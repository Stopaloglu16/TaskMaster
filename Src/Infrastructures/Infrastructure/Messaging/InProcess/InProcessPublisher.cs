namespace Infrastructure.Messaging.InProcess;

/// <summary>
/// Broker-free transport: instead of publishing, it looks up which consumer groups are bound to the
/// message type and dispatches to them inline. Kept so the same saga can be benchmarked with and
/// without RabbitMQ by flipping <c>Messaging:Transport</c> — the outbox, inbox, handlers and saga are
/// identical on both paths, so the difference you measure is the transport and nothing else.
/// <para>
/// It is <em>not</em> equivalent in reliability. There is no retry ladder and no dead-letter queue: a
/// handler that fails has its message dropped, so a bug shows up as a saga stuck in its current state.
/// That limitation is exactly what the RabbitMQ transport exists to remove.
/// </para>
/// </summary>
public sealed class InProcessPublisher<TDbContext>(
    ConsumerRegistry consumers,
    MessageDispatcher<TDbContext> dispatcher,
    ILogger<InProcessPublisher<TDbContext>> logger) : IMessagePublisher
    where TDbContext : OutboxDbContext
{
    public async Task<IReadOnlyList<PublishOutcome>> PublishAsync(
        IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken)
    {
        var outcomes = new List<PublishOutcome>(messages.Count);

        // Serial by design: this transport exists as the baseline the broker path is measured against,
        // and dispatching inline is precisely what makes it a single-threaded pipeline.
        foreach (var message in messages)
        {
            await DispatchAsync(message, cancellationToken);
            outcomes.Add(new PublishOutcome(message, null));
        }

        return outcomes;
    }

    private async Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var groups = consumers.GroupsFor(message.Type).ToList();
        if (groups.Count == 0)
        {
            // Return normally rather than throwing: the relay would otherwise retry this row forever,
            // and no amount of retrying will conjure a subscriber.
            logger.LogWarning("No consumer group handles '{Type}'; dropping.", message.Type);
            return;
        }

        foreach (var group in groups)
        {
            // Note this runs inside the relay's claim transaction, but the dispatcher opens its own
            // scope and connection, so the handler still commits independently — the same at-least-once
            // guarantee as the broker path.
            var outcome = await dispatcher.DispatchAsync(
                group.Name, message.Type, message.Payload, cancellationToken);

            if (outcome.Status is DispatchStatus.Failed or DispatchStatus.Unroutable)
            {
                logger.LogError(
                    "{Group} could not handle {Type} ({Error}); dropped — the in-process transport has no retry ladder.",
                    group.Name, message.Type, outcome.Error);
            }
        }
    }
}
