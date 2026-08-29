using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Messaging;

/// <summary>
/// The "polling publisher" half of the outbox pattern: claims unpublished rows and pushes them to the
/// transport, marking each processed only after the publish is confirmed.
/// <para>
/// Rows are claimed with <c>FOR UPDATE SKIP LOCKED</c>, so this can be scaled to several worker
/// replicas without any of them publishing the same message twice — each transaction takes a disjoint
/// set of rows and any row already locked by a peer is skipped rather than waited on.
/// </para>
/// <para>
/// Publish-then-mark is deliberately at-least-once: a crash after the broker confirms but before the
/// commit re-publishes the message, and the consumer's inbox dedupes it. The reverse order would be
/// at-most-once, i.e. silent message loss.
/// </para>
/// </summary>
public sealed class OutboxRelay<TDbContext>(
    IServiceScopeFactory scopeFactory,
    IMessagePublisher publisher,
    IConfiguration configuration,
    ILogger<OutboxRelay<TDbContext>> logger) : BackgroundService
    where TDbContext : OutboxDbContext
{
    private const int BatchSize = 100;

    /// <summary>
    /// Claim a batch of pending rows and lock them for the life of the transaction. The locking clause
    /// is why this is raw SQL: EF cannot express <c>FOR UPDATE SKIP LOCKED</c>, and it must not be
    /// composed onto with LINQ either — EF would wrap it in a subquery and the lock would be lost.
    /// </summary>
    private static readonly string ClaimSql = $"""
        SELECT * FROM {OutboxDbContext.Schema}.{OutboxDbContext.OutboxTable}
        WHERE "{nameof(OutboxMessage.ProcessedOn)}" IS NULL
        ORDER BY "{nameof(OutboxMessage.Sequence)}"
        LIMIT {BatchSize}
        FOR UPDATE SKIP LOCKED
        """;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Idle poll interval. Only paid when the outbox is empty: a full batch loops straight round to
        // drain the backlog, so throughput is not capped at BatchSize per tick.
        var pollInterval = TimeSpan.FromMilliseconds(
            configuration.GetValue<int?>("Messaging:Outbox:PollIntervalMs") ?? 100);

        while (!stoppingToken.IsCancellationRequested)
        {
            var drainedFullBatch = false;

            try
            {
                drainedFullBatch = await RelayBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Transient DB/broker failure. The claim transaction rolled back, so the rows are still
                // pending and simply get picked up again.
                logger.LogError(ex, "Outbox relay pass failed; will retry.");
            }

            if (drainedFullBatch)
            {
                continue;
            }

            try
            {
                await Task.Delay(pollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <returns><c>true</c> if the batch came back full, meaning there is probably more to drain.</returns>
    private async Task<bool> RelayBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var pending = await db.OutboxMessages.FromSqlRaw(ClaimSql).ToListAsync(ct);
        if (pending.Count == 0)
        {
            return false;
        }

        // One call for the whole batch: the transport pipelines the publishes and collects the confirms,
        // so the batch costs about one broker round-trip rather than one per message.
        var outcomes = await publisher.PublishAsync(pending, ct);
        var published = 0;

        foreach (var (message, error) in outcomes)
        {
            if (error is null)
            {
                message.ProcessedOn = DateTime.UtcNow;
                published++;
                continue;
            }

            // Leave ProcessedOn null so the row is retried. A message that cannot be *published* is an
            // infrastructure problem, not a poison message, so there is no attempt ceiling here — the
            // growing Attempts count and outbox depth are what you alert on.
            message.Attempts++;
            message.LastError = error.Message;
            logger.LogError(error, "Failed to publish {Type} ({Id}); attempt {Attempts}.",
                message.Type, message.Id, message.Attempts);
        }

        await db.SaveChangesAsync(ct);
        logger.LogDebug("Outbox relayed {Published}/{Total} message(s).", published, pending.Count);
        await tx.CommitAsync(ct);

        return pending.Count == BatchSize;
    }
}
