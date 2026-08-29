# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

TaskMaster is a .NET 10 / Blazor Server todo-list app built on a Clean Architecture layout and orchestrated with .NET Aspire. Three user types (Admin, Task User, Read only); a `TaskList` has a due date and owns `TaskItem`s.

**Step-by-step procedures live in `.claude/skills/`, not here** — `add-migration`, `add-saga-step`, `debug-saga`, `run-app`, `add-endpoint`. They load only when the matching task comes up, so they carry the detail (commands, review checklists, known traps) that this file has to stay short enough to omit. Keep orientation here and procedure there; when they disagree, the skill is the one to fix.

## Build, run, test

Everything targets `net10.0` via `Directory.Build.props`; **all** NuGet versions are pinned centrally in `Directory.Packages.props` (`ManagePackageVersionsCentrally`), so add a `<PackageVersion>` there and reference the package without a version in the `.csproj`.

```powershell
dotnet build TaskMaster.sln
dotnet run --project Src\Presentation\TaskMaster.AppHost   # starts everything (Aspire dashboard)
```

Tests (xUnit throughout):

```powershell
dotnet test                                                  # whole solution
dotnet test Tests\Domain.UnitTests                           # one project
dotnet test --filter "FullyQualifiedName~TaskListApiTests"    # one class
dotnet test --filter "DisplayName~Should_Return_Ok"           # one test
```

Test-project characteristics worth knowing before running them:

- `Domain.UnitTests`, `Application.IntegrationTests` — plain, no external deps.
- `WebApi.FunctionalTests`, `WebApiAuth.FunctionalTests` — `WebApplicationFactory<Program>` + **Testcontainers PostgreSql**, so Docker must be running. Auth is stubbed by `TestAuthHandler` registered under the `"IntegrationTest"` scheme.
- `WebsiteApp.BUnitTests` — bUnit component tests, no deps.
- `WebsiteApp.PlaywrightTests` — boots the whole Aspire app in-process via `AspireManager`/`DistributedApplicationTestingBuilder`; needs Docker plus installed Playwright browsers.
- `WebsiteApp.SeleniumTests` — drives Chrome against an **already-running** site (`TestSettings:BaseUrl` in its `appsettings.json`) and logs in with a seeded account. These are not self-contained; don't expect them to pass in a plain `dotnet test`.
- `SharedTestDataLibrary` / `SharedUtilityTestMethods` are helper libraries, not test suites.

## EF Core migrations

PostgreSQL is the only provider. Domain and Identity migrations live in `Infrastructure.PostgresMigrations` (the former `ApplicationDb` subfolder split is kept). There are three DbContexts: `ApplicationDbContext` (domain), `WebIdentityContext` (ASP.NET Identity), and `TickerQDbContext` (migrations for it live in the `WebApi` project, schema `ticker`). **Migrations apply automatically at startup**, each context by the service that owns it: `WebApiAuth` runs `MigrateAsync()` for `WebIdentityContext` + `ApplicationDbContext` (plus `SeedData`), which is why everything else `WaitFor`s it in the AppHost; `WebApi` runs it for `TickerQDbContext` right after `builder.Build()`, resolving the context from the pooled `IDbContextFactory<TickerQDbContext>` that `UseTickerQDbContext` registers. All three share `public.__EFMigrationsHistory`. So an entity change needs only `Add-Migration` — the next F5 applies it. See `Docs/EfCoreCommands.txt`; the Package Manager Console form is:

```powershell
Add-Migration <Name> -Context Infrastructure.Data.ApplicationDbContext -project Src\Infrastructures\Infrastructure.PostgresMigrations -OutputDir Migrations\ApplicationDb
Update-Database -Context Infrastructure.Data.ApplicationDbContext
```

Design-time commands need a reachable connection string: run the AppHost first, or set `$env:ConnectionStrings__taskmasterdb`.

## Architecture

Layering (`Src/Core` → `Src/Infrastructures` → `Src/Presentation`), dependencies point inward:

- **Domain** — entities under `Domain/Entities`, all deriving from `BaseEntity<T>` / `BaseAuditableEntity<T>`. `IsDeleted` is a soft-delete **byte** (0 = live, 1 = deleted, 2 = parent deleted); `ApplicationDbContext.OnModelCreating` installs global query filters on `IsDeleted == 0`, so soft-deleted rows are invisible unless `IgnoreQueryFilters()` is used.
- **Application** — no MediatR despite the CQRS-looking folders. `Aggregates/<X>Aggregate/{Commands,Queries}` holds request records, DTOs, and static `…Mapper` extension methods (hand-written mapping, no AutoMapper). Validation is DataAnnotations on the request records. `Common/Models` holds the shared `CustomResult` / `CustomResult<T>` result type, `CustomError`, and the paging types (`PagingParameters`, `PagingResponse<T>`, `PagingExtersions`). Repository interfaces live in `Application/Repositories` and `Common/Interfaces/IRepository<TEntity,TKey>`.
- **Infrastructure** — EF Core implementations: `ApplicationDbContext` (overrides `SaveChangesAsync` to stamp audit fields from `ICurrentUserService`), the generic `EfCoreRepository<TEntity,TKey>`, per-aggregate repositories, `SeedData`, and `Caching/MemoryCacheService`.
- **ServiceLayer** — the application-service tier the endpoints actually call (`ITaskListService`, `ITaskItemService`, `IUserService`, `IDashboardService`, `IFileJobService`). Business rules live here; services return `CustomResult`, and endpoints translate that into `Ok`/`BadRequest`. This is the layer to change for behaviour, not the API files.

**Result convention:** service methods return `CustomResult`/`CustomResult<T>` rather than throwing; endpoints branch on `IsSuccess` and return `TypedResults.Ok(result.Value)` or `TypedResults.BadRequest(result.CustomError)`. Follow this in new code.

## The FileJob import saga

The bulk CSV import runs as an orchestration saga with a transactional outbox/inbox over RabbitMQ. `PATCH /fileupload/Process/{id}` writes the `FileJob` state change **and** the first outbox row in one transaction, then returns `202` — everything after that happens in `WorkerServiceProcess`.

- **Engine** (`Src/Infrastructures/Infrastructure/Messaging/`, ported from the `AspireAppSagaSingle` sample): `OutboxDbContext` (schema `messaging`), `OutboxRelay<T>` (claims rows `FOR UPDATE SKIP LOCKED`, publishes in **batches** — that batching is a 4.7× throughput decision, not tidiness), `MessageDispatcher<T>` (inbox dedupe + the one transaction each handler runs in), and the RabbitMq/InProcess transports. Switch with `Messaging:Transport`.
- **Contracts and handler seam** live in `Src/Core/Application/Messaging/`; the saga's handlers are in `Src/Infrastructures/ServiceLayer/FileJobs/Saga/`.
- **`FileJob` is the saga instance**: `State` (`SagaState`), `Version` (concurrency token), `CorrelationId`. Happy path `Started → Validated → Processed → Completed`; failure compensates via `Compensating → Cancelled`, deleting anything a partial promotion created (found by `TaskList.SourceFileJobId`).

Five things that are load-bearing — change them and it breaks subtly: the outbox is ordered by the `bigint` `Sequence`, never `OccurredOn`; `ProcessedOn` is set only after a publisher confirm; the inbox key is composite `(MessageId, Consumer)`; `FileJob.Version` is a concurrency token bumped on every transition; and each saga handler returns silently when the job is not in the state it expects (an unexpected state is always a redelivery).

**Handlers must not call `SaveChanges` or open transactions** — the dispatcher does. That is also why they take `ApplicationDbContext` directly rather than the repositories: `EfCoreRepository` saves on every call, which would commit mid-handler. And every `EnrichNpgsqlDbContext` call site passes `DisableRetry = true`, because Npgsql's retrying execution strategy forbids the explicit transactions the messaging layer opens.

### Presentation

- **TaskMaster.AppHost** — Aspire orchestration (`Program.cs`): PostgreSQL (`AddPostgres("postgres").WithDataVolume().WithPgAdmin()` + `AddDatabase("taskmasterdb")`), RabbitMQ (`WithDataVolume().WithManagementPlugin()`), Keycloak, Redis, Papercut SMTP (dev mail), WebApi + Scalar API reference, WebApiAuth, WebsiteApp, and `WorkerServiceProcess` (the saga host — it starts with everything else now).
- **WorkerServiceProcess** — hosts the FileJob import saga: the outbox relay plus the `filejob-saga` and `filejob-worker` consumer groups. It no longer polls the database.
- **WebApi** — minimal APIs only. Endpoint groups are static classes in `Apis/*.cs` (`TaskListApiV1()` etc.) mapped in `Program.cs` under `api/v{apiVersion:apiVersion}/…` with `Asp.Versioning` (v1.0 and v2.0; version from URL segment, `api-version` query, or `X-Version` header) and `.RequireAuthorization()`. Also hosts SignalR (`TaskProgressHub` on `/processHub`, `/processTickerQ`, `/notifications`), TickerQ background jobs, an in-process `IBackgroundTaskQueue` + `TaskProcessingWorker`, OpenTelemetry meter `TaskManagerMetrics` / source `TaskManager`, and `UseGlobalExceptionHandler()`. DI wiring for the whole request stack is in `Extensions/Extensions.cs` (`AddApplicationServices`) — register new repositories/services there. `Program.cs` ends with `public partial class Program { }` so `WebApplicationFactory` can find it.
- **WebApiAuth** — a *separate* controller-based API for identity: register, login/JWT, forgot/reset password. Owns `WebIdentityContext` and ASP.NET Identity config (5 failed attempts → 5-minute lockout) plus `IEmailSender`.
- **WebsiteApp** — Blazor Server (interactive server components). It never touches EF; it calls the two APIs over `HttpClient` through the generic `WebApiService<TRequest,TResponse>`, using named clients `"DefaultClient"` (standard resilience) and `"LongRunningClient"` (hedging), with the JWT held in Blazored.LocalStorage and surfaced through `CustomAuthenticationStateProvider`. UI uses Radzen.Blazor + `TaskMasterRazorClassLibrary` (toasts, modals).
- **ServiceDefaults** — shared Aspire defaults: OpenTelemetry, health checks, service discovery, `AddDefaultAuthentication`, `AddDefaultOpenApi`.

Two APIs, three DbContexts, one PostgreSQL database. Nothing reads a connection string from `appsettings`/user secrets any more: the AppHost injects `ConnectionStrings:taskmasterdb` via `WithReference`. `ApplicationDbContext` also takes `ICurrentUserService`, so it cannot be pooled — every service registers it with `AddDbContext(... UseNpgsql ...)` followed by `builder.EnrichNpgsqlDbContext<T>()` rather than Aspire's pooled `AddNpgsqlDbContext`.

## Conventions

- Lots of commented-out blocks (RabbitMQ, Swagger, alternative TickerQ/DB wiring) are deliberate history — leave them unless the task is about them.
- `.editorconfig` sets block-scoped namespaces as a *silent* suggestion; existing files mix block-scoped and file-scoped. Match the file you're editing.
- `Docs/DrawC4Diagram` generates the C4 model diagrams; not part of the running app.
