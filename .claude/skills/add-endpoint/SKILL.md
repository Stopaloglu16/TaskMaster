---
name: add-endpoint
description: Add or change an HTTP endpoint in TaskMaster's WebApi. Use when asked to "add an endpoint", "add an API route", "expose X over HTTP", "add a GET/POST for Y", or when wiring a new service or repository into the request stack. Covers the minimal-API group pattern, API versioning, the CustomResult convention and where DI registration goes.
---

# Adding a WebApi endpoint

`WebApi` is **minimal APIs only** — no controllers. (`WebApiAuth` is the separate controller-based identity API; don't confuse the two.)

## 1. The endpoint group

Endpoint groups are static classes in `Src/Presentation/WebApi/Apis/*.cs` exposing one extension method:

```csharp
public static class TaskListApi
{
    public static RouteGroupBuilder TaskListApiV1(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetTaskListsWithPagination);
        group.MapPost("/", CreateTaskList);
        return group;
    }

    public static async Task<Results<Ok<TaskListDto>, BadRequest<string>>> CreateTaskList(
        CreateTaskListRequest request,
        [FromServices] ITaskListService taskListService,
        CancellationToken cancellationToken)
    { ... }
}
```

Handlers are **static methods** returning `Results<Ok<T>, BadRequest<string>>`. Follow the file you are editing for block- vs file-scoped namespaces — the repo mixes both and `.editorconfig` only makes it a silent suggestion.

## 2. Map it in `Program.cs`

Register the group with the shared version set:

```csharp
var taskList = app.MapGroup("api/v{apiVersion:apiVersion}/tasklist")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1.0)
    .HasApiVersion(2.0);
taskList.TaskListApiV1().RequireAuthorization();
```

Versions are v1.0 and v2.0, resolved from the URL segment, an `api-version` query parameter, or an `X-Version` header. `.RequireAuthorization()` unless there is a stated reason not to.

## 3. The result convention

**Service methods return `CustomResult` / `CustomResult<T>` rather than throwing.** The endpoint only translates:

```csharp
return result.IsSuccess
    ? TypedResults.Ok(result.Value)
    : TypedResults.BadRequest(result.Error);
```

## 4. Business logic goes in ServiceLayer, not the API file

`Src/Infrastructures/ServiceLayer/` is the tier endpoints call (`ITaskListService`, `ITaskItemService`, `IUserService`, `IDashboardService`, `IFileJobService`). Change behaviour there. The API file should stay a thin translation layer.

Data access sits behind repository interfaces in `Application/Repositories`, implemented in `Infrastructure/Repositories` over the generic `EfCoreRepository<TEntity,TKey>`.

## 5. Register in DI

Repositories and services go in `Src/Presentation/WebApi/Extensions/Extensions.cs` (`AddApplicationServices`), alongside the existing `AddScoped` pairs.

If `WorkerServiceProcess` also needs the service, register it **separately** in `Src/WorkerServiceProcess/Program.cs` — the two hosts do not share a registration method.

## 6. Validation

DataAnnotations on the request records in `Application/Aggregates/<X>Aggregate/Commands/`. There is **no MediatR and no FluentValidation** despite the CQRS-shaped folder layout, and mapping is hand-written `…Mapper` extension methods, not AutoMapper.

## 7. Long-running work

Don't block the request. Start a saga and return `202 Accepted` with a status URL — `StartFileJobSaga` in `FileJobUploadApi.cs` is the pattern: write the state change and the first outbox row in one transaction, return immediately, let the worker do the rest. See the `add-saga-step` skill.

For anything that needs progress reporting, SignalR hubs are already mapped (`/processHub`, `/processTickerQ`, `/notifications`).
