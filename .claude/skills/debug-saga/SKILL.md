---
name: debug-saga
description: Diagnose a stuck, failed or duplicated FileJob import. Use when a bulk upload never completes, a job sits in one state, task lists were not created or were created twice, messages pile up in the outbox, a handler keeps retrying, or when asked to "check the outbox", "why is the import stuck", "look at the dead letter queue", or to inspect saga state.
---

# Debugging the FileJob saga

Work outwards: saga state → outbox → inbox → dead-letter queues. Most stalls are answered by the first two.

## 0. Is the transport what you think it is?

```
Messaging:Transport = RabbitMq | InProcess
```

**Under `InProcess` there is no retry ladder and no DLQ — a failed handler's message is logged and dropped**, and the saga simply stalls in place. Don't hunt for a dead-letter that cannot exist. Check `Src/WorkerServiceProcess/appsettings.json` or the `Messaging__Transport` environment variable first.

Also confirm `workerserviceprocess` is actually running in the Aspire dashboard. It used to have `WithExplicitStart()`, which meant nothing consumed the pipeline unless someone pressed Start.

## 1. Saga state

```sql
SELECT "Id","State","Version","IsCompleted","FailureReason" FROM "FileJobs" ORDER BY "Id" DESC LIMIT 10;
SELECT "FileRowType", COUNT(*) FROM "FileJobUploads" WHERE "FileJobId" = :id GROUP BY 1;
```

`State` is stored as text on purpose so it reads directly. `FileRowType`: 0 NewUpload, 1 ValidateIssue, 2 Validated, 3 Processed, 4 ProcessIssue, 5 MovedToLive.

| Symptom | Where to look next |
|---|---|
| Stuck in `NewUpload` | The saga was never started — `PATCH /fileupload/Process/{id}` not called |
| Stuck in `Started` | `ValidateFile` unpublished (outbox) or dead-lettered |
| Stuck in `Validated` / `Processed` | That step's command failed permanently — check `filejob-worker.dead` |
| `Cancelled` with a `FailureReason` | Working as designed. Read the reason; it names the business problem |
| `Completed` but no task lists | Should be impossible now — `IsCompleted` is set only on the `PromotedToLive` reply. If you see it, that invariant broke |

## 2. Outbox — has it been published?

```sql
SELECT "Sequence","Type","CorrelationId","ProcessedOn" IS NOT NULL AS published,"Attempts","LastError"
FROM messaging.outbox_messages ORDER BY "Sequence" DESC LIMIT 20;
```

- `ProcessedOn IS NULL` → not yet confirmed by the broker. It is stamped **only** after a publisher confirm, so a null here means the message genuinely is not durably on the wire.
- Rising `Attempts` + a populated `LastError` → the broker is unreachable or rejecting. **There is deliberately no attempt ceiling on the publish side** — an unpublishable message is an infrastructure problem, not a poison message. Outbox depth and `Attempts` are what you alert on.
- Ordering is by the `bigint` `Sequence`, never `OccurredOn`.

## 3. Inbox — was it consumed, or deduped?

```sql
SELECT "MessageId","Consumer","Type","ReceivedOn" FROM messaging.inbox_messages ORDER BY "ReceivedOn" DESC LIMIT 20;
```

Keyed `(MessageId, Consumer)`. A message present for one consumer group and absent for another is **normal fan-out**, not a bug — each group must be allowed to process a message once.

A published outbox row with no matching inbox row and no DLQ entry means the consumer never got it: check queue bindings and that the handler is registered in a consumer group.

## 4. Dead-letter queues — poison messages

Retry ladder: `filejob.retry.5s` → `.30s` → `.120s`, giving **4 deliveries total** before parking. Then:

- `filejob-worker.dead` — a participant command failed 4 times
- `filejob-saga.dead` — an orchestrator reply failed 4 times

The reason is in the **`x-error` header**, and the republished message keeps its original `MessageId` so the inbox still protects a manual replay.

```powershell
docker exec <rabbit-container> rabbitmqctl -q list_queues name messages
```

Or open the RabbitMQ management UI from the Aspire dashboard (`rabbitmq` resource) and use Get Message on the `.dead` queue to read the header.

Worker logs carry the same information: `{Queue}: {Type} failed … dead-lettering`. Note the HTTP response returned `202 Accepted` long before any of this, so the API tells you nothing about the outcome.

## 5. Replaying

Republish from the management UI. The inbox will suppress it if that consumer group already handled it — expect a `treating as duplicate` log line and no side effects. That is the correct outcome, not a failure to replay.

## Notes

- `outbox_messages` and `inbox_messages` are never purged; they grow forever. Large tables are expected, not a symptom.
- Every `EnrichNpgsqlDbContext` call passes `DisableRetry = true`, because the messaging layer opens explicit transactions. If you see `The configured execution strategy 'NpgsqlRetryingExecutionStrategy' does not support user-initiated transactions`, a call site is missing it.
