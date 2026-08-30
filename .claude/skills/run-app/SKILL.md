---
name: run-app
description: Launch TaskMaster locally via the .NET Aspire AppHost and find the dashboard, database, queues and dev mailbox. Use when asked to run, start, launch, or debug the app, to reproduce something in the running site, to reset the database to a clean schema, or when a run fails because Docker or a port is unavailable.
---

# Running TaskMaster

## Prerequisite: Docker

The AppHost starts PostgreSQL, RabbitMQ, Keycloak, Redis and Papercut as containers. **Docker Desktop must be running before you start** — without it the AppHost fails with `failed to connect to the docker API at npipe:////./pipe/dockerDesktopLinuxEngine`.

## Start

```powershell
dotnet run --project Src\Presentation\TaskMaster.AppHost
```

The dashboard opens automatically (`https` launch profile). Everything else is reached from there — don't guess service ports, they are assigned per run.

## What comes up

Containers: `postgres` (+ `postgres-pgadmin`), `rabbitmq` (management plugin), `keycloak`, `redis`, `papercut`.
Projects: `webapiauth`, `webapi` (+ `scalar` API reference), `websiteapp`, `workerserviceprocess`.

Startup order is wired and matters: **`webapiauth` applies the domain and identity migrations and seeds data**, so `webapi` and `workerserviceprocess` `WaitFor` it. `webapi` applies the TickerQ migration itself.

`workerserviceprocess` is the saga host (outbox relay + the `filejob-saga` and `filejob-worker` consumer groups) and starts with everything else. It no longer polls the database.

## Where to look for what

| Need | Go to |
|---|---|
| Tables, rows, the `messaging` and `ticker` schemas | pgAdmin (`postgres-pgadmin` resource) |
| Queues, dead-letters, message counts | RabbitMQ management UI (`rabbitmq` resource) |
| API surface, try an endpoint | Scalar (`scalar` resource) |
| Emails the app "sent" | Papercut (`papercut` resource) |
| Logs, traces, metrics | Aspire dashboard itself |

## Data persists between runs

`AddPostgres("postgres").WithDataVolume()` mounts a named Docker volume, so records survive stopping the AppHost, restarting Docker and rebooting. The container is recreated each run; the volume is not.

To get a genuinely clean schema, stop the AppHost and drop the volume. **Find the name rather than hardcoding it** — Aspire hashes it:

```powershell
docker volume ls | Select-String "postgres-data"
docker volume rm <the-name-you-found>
```

Migrations then reapply from scratch on the next run.

## Known gotchas

- **Keycloak is pinned to host port 8080** so its OIDC issuer URL stays stable across runs. If something else on the machine holds 8080, that resource fails to start. The issuer is baked into every token, so `http://localhost:8080/realms/taskmaster` is used verbatim everywhere rather than the Aspire service-discovery hostname — reaching Keycloak by a different URL produces an `iss` WebApi rejects.
- **Editing `TaskMaster.AppHost/Keycloak/taskmaster-realm.json` does nothing until you drop the Keycloak volume.** `WithRealmImport` only imports a realm the server does not already have, and `.WithDataVolume()` keeps the old one. Same drill as the Postgres volume above:

  ```powershell
  docker volume ls | Select-String "keycloak-data"
  docker volume rm <the-name-you-found>
  ```

  Failing to do this looks exactly like the realm JSON being wrong.
- **Keycloak is the identity provider.** Sign in as `adminuser` / `taskuser` / `readonly`, password `SuperStrongPassword+123` (seeded by the realm import). WebApiAuth no longer signs tokens — it brokers to the realm's `taskmaster-auth` client — and WebApi validates RS256 tokens against the realm JWKS. The admin console is at `http://localhost:8080`, user `admin`, password in AppHost user-secrets under `Parameters:keycloak-password`.
- Password-reset mail comes from Keycloak itself (realm SMTP → the Papercut container), not from `IEmailSender`. Read it in Papercut from the dashboard. The realm points at **`papercut:2525`**, not `papercut:25` — Keycloak reaches Papercut container-to-container, and the `changemakerstudiosus/papercut-smtp` image listens on 2525 internally; the `25` in `AddPapercutSmtp("papercut", 80, 25)` is the *host* port. Getting this wrong shows up only as `execute-actions-email -> 500` with `Connection refused` in the Keycloak log.
- The `taskmaster-auth` service account needs **`view-realm`** on top of `manage-users`/`view-users`/`query-users`. Assigning a realm role requires reading the role definition first, and the `manage-users` family does not grant that — without it registration fails at the role assignment with a 403 on `GET /admin/realms/taskmaster/roles/{name}`.
- RabbitMQ is provisioned and WebApi holds a connection, but `RabbitPublisher`/`RabbitConsumer` in `WebApi/RabbitMq/` stay commented out — they are unfinished drafts with real bugs. The saga uses its own messaging engine, not those.
- If startup throws `42P01: relation "ticker.CronTickers" does not exist`, TickerQ's migration has not been applied — see the `add-migration` skill.

## Just building or testing

```powershell
dotnet build TaskMaster.sln
```

Self-contained test suites (no Docker): `Domain.UnitTests`, `Application.IntegrationTests`, `Infrastructure.IntegrationTests`, `WebsiteApp.BUnitTests`. The functional suites need Docker; `WebsiteApp.SeleniumTests` needs an already-running site and is not self-contained.
