---
name: add-saga-step
description: Add or change a step in the FileJob import saga — a new message contract, message handler, consumer group binding, state transition or compensation. Use when asked to "add a saga step", "add a message/handler", "add a state to the import", "make X happen after the file is processed", or when changing anything under ServiceLayer/FileJobs/Saga or Application/Messaging. Encodes rules that fail silently rather than loudly.
---

# Adding a step to the FileJob saga

The saga orchestrates `Started → Validated → Processed → Completed`, with `Compensating → Cancelled` on failure. `FileJob` is the saga instance row. The engine lives in `Src/Infrastructures/Infrastructure/Messaging/` (ported from the `AspireAppSagaSingle` sample) and knows nothing about file jobs.

## Where things go

| Piece | Location |
|---|---|
| Message contract | `Src/Core/Application/Messaging/Contracts/FileJobMessages.cs` |
| Handler | `Src/Infrastructures/ServiceLayer/FileJobs/Saga/` |
| Consumer group binding | `Src/Infrastructures/ServiceLayer/FileJobs/Saga/FileJobSagaModule.cs` |
| Host registration | `Src/WorkerServiceProcess/Program.cs` |

## The rules that break things silently

**1. Every contract carries `Guid MessageId`, `Guid CorrelationId`, `int FileJobId`** and implements `IMessage`. `MessageId` is the receiving inbox's idempotency key; `CorrelationId` is `FileJob.CorrelationId`.

**2. Handlers must never call `SaveChanges` or open a transaction.** `MessageDispatcher` wraps each handler in a transaction that inserts the inbox row, runs the handler, and commits once. A `SaveChanges` inside a handler commits early and breaks the atomicity the whole pattern rests on.

**3. Handlers take `ApplicationDbContext` directly, not the repositories.** `EfCoreRepository` calls `SaveChangesAsync` on every method, so using it here would violate rule 2. This is deliberate, not an oversight.

**4. Orchestrator handlers guard on the expected state and return silently otherwise:**

```csharp
var job = await FindAsync(message.FileJobId, cancellationToken);
if (job is null || job.State != SagaState.Validated) { return; }
```

That looks like it would discard an early reply, but it cannot: a command is only ever enqueued in the same transaction as the transition preceding it, so the state is durably in place before the command is publishable, and therefore before any reply to it can exist. **An unexpected state is always a redelivery of something already applied.** Preserve this guard when adding steps.

**5. Bump `job.Version` on every transition** — it is an `IsConcurrencyToken`, which turns two replies racing for one job into a compare-and-swap. The loser gets `DbUpdateConcurrencyException`, which the dispatcher converts into a retryable `Failed`. `FileJobSagaHandler.Transition()` already does this; use it.

**6. Distinguish business failure from infrastructure failure.**

- *Business* (retrying cannot fix it — empty file, no assignable rows, duplicate item titles): `db.Enqueue(new …Failed(...))` and `return`. The saga cancels cleanly with a `FailureReason`.
- *Infrastructure* (broker down, deadlock, transient DB error): let it **throw**. It goes onto the retry ladder and then the dead-letter queue.

Getting this backwards means either a poison message retried forever, or a real outage silently marked as a cancelled job.

**7. Register in the right consumer group** in `FileJobSagaModule.cs`:

```csharp
.Handles<TheHandler, TheMessage>()
```

- `filejob-saga` (prefetch **8**) — orchestrator reply events. The low prefetch is deliberate: every message here contends for the same `FileJob` row, so more concurrency buys concurrency conflicts, not throughput.
- `filejob-worker` (prefetch 16) — participant commands that do the actual work.

One `.Handles<>()` line registers the handler, records the type for deserialization, **and** creates the queue binding.

**8. Registration order in `Src/WorkerServiceProcess/Program.cs` is load-bearing:**

```
AddMessagingCore<ApplicationDbContext>()  →  AddFileJobSagaModule() / AddFileJobWorkerModule()  →  AddMessageTransport<ApplicationDbContext>()
```

The broker topology is derived from the registered consumer groups, and hosted services start in registration order. `AddMessagingCore` before any `AddConsumer` is enforced with an exception; the rest is not.

**9. If a new step writes rows a later failure must undo, record provenance.** `TaskList.SourceFileJobId` exists so `RollbackPromotionHandler` can find and delete exactly what a failed promotion created. Without it, compensation has nothing to target.

## Worked example

`PromoteToLiveHandler` (participant) and `FileJobSagaHandler` (orchestrator) together show every rule above: pre-checking a business failure, idempotency belt-and-braces, enqueuing the next message, and transitioning without saving.

## After changing contracts

Message identity on the wire is the bare CLR type name — used as the AMQP `type`, the topic routing key **and** the registry key at once. Renaming a contract type is a breaking change for any message already sitting in a queue or the outbox. There is no versioning story; drain first or accept the loss.

New state on `FileJob` means a migration — see the `add-migration` skill.
