---
name: add-migration
description: Create, review and apply an EF Core migration in TaskMaster. Use when changing an entity, adding a DbSet or column, or when asked to "add a migration", "update the database", "scaffold a migration", or when a startup fails with a pending-model-changes or missing-relation error. Covers all three DbContexts (ApplicationDbContext, WebIdentityContext, TickerQDbContext) and the review step that catches EF's data-destroying guesses.
---

# Adding an EF Core migration

PostgreSQL is the only provider. There are **three DbContexts across two migration projects**, and they do not share commands.

## 1. Pick the right project and output directory

| Context | `--project` | `--startup-project` | `-o` |
|---|---|---|---|
| `Infrastructure.Data.ApplicationDbContext` | `Src\Infrastructures\Infrastructure.PostgresMigrations` | `Src\Presentation\WebApiAuth` | `Migrations\ApplicationDb` |
| `Infrastructure.Data.WebIdentityContext` | `Src\Infrastructures\Infrastructure.PostgresMigrations` | `Src\Presentation\WebApiAuth` | `Migrations` |
| `TickerQDbContext` | `Src\Presentation\WebApi` | `Src\Presentation\WebApi` | `Migrations` |

`TickerQDbContext` comes from the `TickerQ.EntityFrameworkCore` package, not this repo, and its migrations live in the WebApi assembly (`MigrationsAssembly("WebApi")` in `WebApi/Program.cs`). Its tables are in the `ticker` schema.

All three share `public.__EFMigrationsHistory`.

## 2. Give design-time a connection string

There is **no `IDesignTimeDbContextFactory`** in this repo, so `dotnet ef` boots the real host and needs the Aspire-injected connection string. Either run the AppHost first, or set it yourself:

```powershell
$env:ConnectionStrings__taskmasterdb = "Host=localhost;Port=5432;Database=taskmasterdb;Username=postgres;Password=postgres"
```

## 3. Add it

```powershell
dotnet ef migrations add <Name> --context Infrastructure.Data.ApplicationDbContext `
  --project Src\Infrastructures\Infrastructure.PostgresMigrations `
  --startup-project Src\Presentation\WebApiAuth -o Migrations\ApplicationDb
```

`Docs\EfCoreCommands.txt` has the Package Manager Console forms.

> **Never pass `--no-build` immediately after adding a migration.** It runs against the stale assembly and reports `No migrations were found` or `already up to date` while doing nothing at all. This looks exactly like a broken migration and wastes real time. Build first, or just omit the flag.

## 4. Review the scaffolded file before applying it — this is the important step

EF's guesses have been wrong here in three specific ways, each of which destroys data or fails outright on a non-empty table:

1. **A `RenameColumn` pairing two unrelated columns.** EF matched a dropped `FileJobType` (an int enum) with a new `Version` (a concurrency counter) purely on type and position. Every job would have silently inherited its old enum value as a version number.
2. **A new non-nullable column with a constant default, sitting under a unique index.** `CorrelationId` defaulted to `Guid.Empty` and `BatchKey` to `""` — fine on an empty table, but the very next `CreateIndex ... unique: true` fails the moment two rows exist.
3. **An enum-stored-as-text column defaulted to `""`**, which is not a valid member and blows up on materialisation.

**The fix pattern is already in the repo** — read `Migrations\ApplicationDb\20260829192327_AddFileJobSagaAndOutbox.cs`:

- replace a bogus `RenameColumn` with an explicit `AddColumn` + `DropColumn`;
- put `migrationBuilder.Sql(...)` backfills **before** the `CreateIndex` calls (`gen_random_uuid()` for correlation ids, `'legacy-' || "Id"::text` for per-row keys, a `CASE` to map old enum values onto new ones);
- mirror all of it in `Down`.

Also check: a column made nullable should be `AlterColumn` (not drop/add), and a sensible non-empty `defaultValue` for any new required column.

## 5. Test against data, not an empty database

An empty table hides every hazard above. Roll back to the previous migration, insert rows that would collide, then apply:

```powershell
docker run -d --name tm-pg-test -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=taskmasterdb -p 55432:5432 postgres:17
$env:ConnectionStrings__taskmasterdb = "Host=localhost;Port=55432;Database=taskmasterdb;Username=postgres;Password=postgres"
dotnet ef database update <PreviousMigration> --context ... --project ... --startup-project ...
# insert at least two rows in each affected table, then:
dotnet ef database update --context ... --project ... --startup-project ...
```

Verify the backfilled values are actually distinct where a unique index requires it. Clean up with `docker rm -f tm-pg-test`.

## 6. Applying at runtime

You usually don't need `database update` — migrations apply on startup. `WebApiAuth` migrates `WebIdentityContext` + `ApplicationDbContext` (and seeds); `WebApi` migrates `TickerQDbContext`. Everything else `WaitFor`s `webapiauth` in the AppHost.
