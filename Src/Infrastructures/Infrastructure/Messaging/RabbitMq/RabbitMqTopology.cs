using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Declares the whole broker topology from the registered consumer groups, before the relay or any
/// consumer starts. Declarations are idempotent, so every process can safely assert the same topology
/// on boot — that is what makes the system self-provisioning instead of dependent on a manual setup step.
/// <para>
/// Shape, for consumer groups <c>orders</c> / <c>inventory</c> / …:
/// </para>
/// <code>
///                        ┌─ (routing key = message type) ─→ [orders]    ─┐
///   outbox ─→ (saga: topic) ──────────────────────────────→ [inventory] ─┤
///                        └──────────────────────────────────→ [payment] ─┤ handler fails
///                                                                        ↓
///          [saga.retry.5s] ←── (saga.retry.5s: fanout) ←── republish, rung 0
///                 │ TTL 5s expires
///                 ↓  dead-letters, original routing key = group name
///        (saga.requeue: direct) ─→ back to that one group's queue only
///
///   ladder exhausted ─→ (saga.dlx: direct) ─→ [orders.dead] / [inventory.dead] / …
/// </code>
/// </summary>
public sealed class RabbitMqTopology(
    IConnection connection,
    ConsumerRegistry consumers,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqTopology> logger) : IHostedService
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Primary exchange: topic, so a message type can be bound to more than one group without the
        // publisher knowing who listens. The relay publishes with routing key = message type name.
        await channel.ExchangeDeclareAsync(
            _options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);

        // Requeue + dead-letter exchanges are direct and routed by consumer-group name.
        await channel.ExchangeDeclareAsync(
            _options.RequeueExchange, ExchangeType.Direct, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            _options.DeadLetterExchange, ExchangeType.Direct, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);

        await DeclareRetryLadderAsync(channel, cancellationToken);

        foreach (var group in consumers.Groups)
        {
            await DeclareConsumerGroupAsync(channel, group, cancellationToken);
        }

        logger.LogInformation(
            "Declared RabbitMQ topology on exchange '{Exchange}' for {Count} consumer group(s): {Groups}.",
            _options.Exchange, consumers.Groups.Count, string.Join(", ", consumers.Groups.Select(g => g.Name)));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task DeclareRetryLadderAsync(IChannel channel, CancellationToken ct)
    {
        for (var rung = 0; rung < _options.RetryDelaysSeconds.Length; rung++)
        {
            var name = _options.RetryQueue(rung);

            await channel.ExchangeDeclareAsync(
                _options.RetryExchange(rung), ExchangeType.Fanout, durable: true, autoDelete: false,
                cancellationToken: ct);

            // The queue itself is the timer: nothing consumes it, messages simply expire and are
            // dead-lettered onward to the requeue exchange, keeping their original routing key
            // (the consumer group name) so they land back in exactly the queue that failed.
            await channel.QueueDeclareAsync(
                name, durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-message-ttl"] = _options.RetryDelaysSeconds[rung] * 1000,
                    ["x-dead-letter-exchange"] = _options.RequeueExchange,
                },
                cancellationToken: ct);

            await channel.QueueBindAsync(name, _options.RetryExchange(rung), routingKey: string.Empty,
                cancellationToken: ct);
        }
    }

    private async Task DeclareConsumerGroupAsync(IChannel channel, ConsumerGroup group, CancellationToken ct)
    {
        await channel.QueueDeclareAsync(
            group.Name, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);

        // One binding per message type the group handles. This is the module's subscription list, and
        // it comes straight from its AddConsumer(...) registration.
        foreach (var messageType in group.MessageTypes)
        {
            await channel.QueueBindAsync(group.Name, _options.Exchange, messageType.Name, cancellationToken: ct);
        }

        // Retries come back through here, addressed to this group alone.
        await channel.QueueBindAsync(group.Name, _options.RequeueExchange, group.Name, cancellationToken: ct);

        var dead = _options.DeadLetterQueue(group.Name);
        await channel.QueueDeclareAsync(
            dead, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await channel.QueueBindAsync(dead, _options.DeadLetterExchange, group.Name, cancellationToken: ct);
    }
}
