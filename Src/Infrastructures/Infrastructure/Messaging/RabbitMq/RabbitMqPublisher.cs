using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Publishes outbox rows to the topic exchange, and is also the retry/dead-letter republish path for
/// the consumer.
/// <para>
/// Two things here are what make the outbox actually safe:
/// <list type="bullet">
/// <item><b>Publisher confirms.</b> The channel is created with confirmation tracking, so
/// <c>BasicPublishAsync</c> does not complete until the broker has acked the message. Only then does
/// the relay mark the row processed.</item>
/// <item><b>mandatory + persistent.</b> Persistent survives a broker restart; mandatory turns "no
/// queue is bound to this routing key" into an exception instead of a silently discarded message —
/// which would otherwise be an invisible way for a saga to stall forever.</item>
/// </list>
/// </para>
/// </summary>
public sealed class RabbitMqPublisher(
    IConnection connection,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqPublisher> logger) : IMessagePublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;

    // An IChannel must not have concurrent publishes in flight, and this instance is a singleton shared
    // by the relay and every consumer's failure path. One channel + one gate is simpler to reason about
    // than a channel pool, and the relay — the only hot publisher — is single-threaded anyway.
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IChannel? _channel;

    public async Task<IReadOnlyList<PublishOutcome>> PublishAsync(
        IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken)
    {
        var outcomes = new List<PublishOutcome>(messages.Count);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var channel = await GetChannelAsync(cancellationToken);

            // Phase 1: put every publish in flight without awaiting. Each returned ValueTask completes
            // when *that* message's confirm arrives, so the broker's fsync and round-trip overlap across
            // the whole batch instead of being paid serially. Routing key is the message type name; the
            // bindings decide which consumer groups receive it.
            var inFlight = new List<(OutboxMessage Message, ValueTask Confirm)>(messages.Count);
            foreach (var message in messages)
            {
                inFlight.Add((message, channel.BasicPublishAsync(
                    _options.Exchange, message.Type, mandatory: true,
                    basicProperties: BuildProperties(message),
                    body: Encoding.UTF8.GetBytes(message.Payload),
                    cancellationToken: cancellationToken)));
            }

            // Phase 2: collect the confirms. Failures are per-message — one unroutable message must not
            // cost the rest of the batch their acknowledgement.
            foreach (var (message, confirm) in inFlight)
            {
                try
                {
                    await confirm;
                    outcomes.Add(new PublishOutcome(message, null));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    outcomes.Add(new PublishOutcome(message, ex));
                }
            }
        }
        finally
        {
            _gate.Release();
        }

        return outcomes;
    }

    private static BasicProperties BuildProperties(OutboxMessage message) => new()
    {
        MessageId = message.Id.ToString(),
        CorrelationId = message.CorrelationId.ToString(),
        Type = message.Type,
        ContentType = "application/json",
        DeliveryMode = DeliveryModes.Persistent,
        Timestamp = new AmqpTimestamp(new DateTimeOffset(message.OccurredOn, TimeSpan.Zero).ToUnixTimeSeconds()),
    };

    /// <summary>
    /// Raw publish, used by the consumer to move a failed delivery onto a retry rung or the
    /// dead-letter exchange. Awaiting the confirm here is what lets the consumer safely ack the
    /// original delivery afterwards: the message exists in its new home before the old copy is released.
    /// </summary>
    public async Task PublishAsync(
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        BasicProperties properties,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var channel = await GetChannelAsync(cancellationToken);
            await channel.BasicPublishAsync(
                exchange, routingKey, mandatory: true, basicProperties: properties, body: body,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken ct)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        if (_channel is not null)
        {
            logger.LogWarning("Publisher channel was closed; reopening.");
            await _channel.DisposeAsync();
        }

        _channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken: ct);

        return _channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        _gate.Dispose();
    }
}
