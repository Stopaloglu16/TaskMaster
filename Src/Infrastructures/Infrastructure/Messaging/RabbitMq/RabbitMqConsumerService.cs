using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Runs one AMQP consumer per registered consumer group and turns each delivery's outcome into a
/// delivery-semantics decision. This is the part the old in-process bus could not have: instead of
/// swallowing a handler exception and dropping the message, a failure is retried on a backoff ladder
/// and only then parked in a dead-letter queue.
/// <para>
/// Every path ends in an <c>ack</c>, including the failures — because the retry and dead-letter
/// republishes are confirmed <em>before</em> the ack, the message is always somewhere durable before
/// this copy is released. <c>nack</c> is reserved for the case where that republish itself failed,
/// where requeuing is the only way not to lose the message.
/// </para>
/// </summary>
public sealed class RabbitMqConsumerService<TDbContext>(
    IConnection connection,
    ConsumerRegistry consumers,
    RabbitMqPublisher publisher,
    MessageDispatcher<TDbContext> dispatcher,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConsumerService<TDbContext>> logger) : BackgroundService
    where TDbContext : OutboxDbContext
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly List<IChannel> _channels = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var group in consumers.Groups)
        {
            // consumerDispatchConcurrency lets the client library run this group's deliveries in
            // parallel; prefetch caps how many the broker will hand over unacked. Together they are the
            // group's throughput knob — and the reason the inbox needs its race guard.
            var channel = await connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: false,
                    publisherConfirmationTrackingEnabled: false,
                    consumerDispatchConcurrency: group.Prefetch),
                cancellationToken: stoppingToken);

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: group.Prefetch, global: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            var captured = group;
            consumer.ReceivedAsync += (_, delivery) => OnDeliveryAsync(captured, channel, delivery, stoppingToken);

            await channel.BasicConsumeAsync(captured.Name, autoAck: false, consumer: consumer,
                cancellationToken: stoppingToken);

            _channels.Add(channel);
            logger.LogInformation(
                "Consuming '{Queue}' (prefetch {Prefetch}) for {Types}.",
                group.Name, group.Prefetch, string.Join(", ", group.MessageTypes.Select(t => t.Name)));
        }

        // Deliveries arrive on the client library's threads; nothing to do here but stay alive.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnDeliveryAsync(
        ConsumerGroup group, IChannel channel, BasicDeliverEventArgs delivery, CancellationToken ct)
    {
        // Prefer the AMQP type property, fall back to the routing key — they are the same value, but a
        // message replayed by hand from the management UI may only have the routing key.
        var typeName = delivery.BasicProperties.Type ?? delivery.RoutingKey;
        var failedAttempts = ReadAttempt(delivery.BasicProperties);

        try
        {
            var payload = Encoding.UTF8.GetString(delivery.Body.Span);
            var outcome = await dispatcher.DispatchAsync(group.Name, typeName, payload, ct);

            switch (outcome.Status)
            {
                case DispatchStatus.Handled:
                case DispatchStatus.Duplicate:
                    break;

                case DispatchStatus.Unroutable:
                    // No handler or no CLR type: a redelivery would fail identically, so skip the ladder.
                    logger.LogError("{Queue}: {Type} is unroutable ({Error}); dead-lettering.",
                        group.Name, typeName, outcome.Error);
                    await DeadLetterAsync(group, delivery, outcome.Error, ct);
                    break;

                case DispatchStatus.Failed when failedAttempts < _options.MaxRetries:
                    await RetryAsync(group, delivery, failedAttempts, ct);
                    break;

                case DispatchStatus.Failed:
                    logger.LogError("{Queue}: {Type} failed {Attempts} times ({Error}); dead-lettering.",
                        group.Name, typeName, failedAttempts + 1, outcome.Error);
                    await DeadLetterAsync(group, delivery, outcome.Error, ct);
                    break;
            }

            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken: ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // We could not get the message anywhere durable (broker republish failed, channel died).
            // Requeue is the only option that does not lose it; the inbox makes the redelivery safe.
            logger.LogError(ex, "{Queue}: could not settle {Type}; requeuing.", group.Name, typeName);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: true,
                cancellationToken: CancellationToken.None);
        }
    }

    /// <summary>Park the message on the next rung of the ladder; it comes back after that rung's TTL.</summary>
    private async Task RetryAsync(
        ConsumerGroup group, BasicDeliverEventArgs delivery, int failedAttempts, CancellationToken ct)
    {
        var delay = _options.RetryDelaysSeconds[failedAttempts];
        logger.LogWarning("{Queue}: {Type} failed (attempt {Attempt}); retrying in {Delay}s.",
            group.Name, delivery.BasicProperties.Type ?? delivery.RoutingKey, failedAttempts + 1, delay);

        var properties = Clone(delivery.BasicProperties);
        properties.Headers![RabbitMqOptions.AttemptHeader] = failedAttempts + 1;

        // Routing key is the group name, not the message type: the retry queue dead-letters onward to
        // the direct requeue exchange, which must deliver only back to the group that failed.
        await publisher.PublishAsync(
            _options.RetryExchange(failedAttempts), group.Name, delivery.Body, properties, ct);
    }

    private async Task DeadLetterAsync(
        ConsumerGroup group, BasicDeliverEventArgs delivery, string? error, CancellationToken ct)
    {
        var properties = Clone(delivery.BasicProperties);
        properties.Headers![RabbitMqOptions.ErrorHeader] = error ?? "unknown";

        await publisher.PublishAsync(_options.DeadLetterExchange, group.Name, delivery.Body, properties, ct);
    }

    /// <summary>
    /// Copy the inbound properties so republished messages keep their identity — the message id in
    /// particular, since that is the receiving inbox's idempotency key and a fresh one would defeat it.
    /// </summary>
    private static BasicProperties Clone(IReadOnlyBasicProperties source) => new()
    {
        MessageId = source.MessageId,
        CorrelationId = source.CorrelationId,
        Type = source.Type,
        ContentType = source.ContentType,
        DeliveryMode = DeliveryModes.Persistent,
        Headers = source.Headers is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(source.Headers),
    };

    private static int ReadAttempt(IReadOnlyBasicProperties properties) =>
        properties.Headers?.TryGetValue(RabbitMqOptions.AttemptHeader, out var raw) == true
            ? raw switch
            {
                int i => i,
                long l => (int)l,
                byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed) => parsed,
                _ => 0,
            }
            : 0;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        foreach (var channel in _channels)
        {
            await channel.DisposeAsync();
        }

        _channels.Clear();
    }
}
