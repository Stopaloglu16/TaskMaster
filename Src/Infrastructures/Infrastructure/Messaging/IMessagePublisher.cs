namespace Infrastructure.Messaging;

/// <summary>What happened to one message in a published batch. A null <paramref name="Error"/> means confirmed.</summary>
public readonly record struct PublishOutcome(OutboxMessage Message, Exception? Error);

/// <summary>
/// Transport seam: takes committed outbox rows and gets them onto the wire.
/// <para>
/// The unit is a <em>batch</em>, not a message, and that is a performance decision rather than a
/// convenience one. A durable publish is only safe once the broker has confirmed it, and a confirm costs
/// a network round-trip plus the broker's fsync. Awaiting them one at a time serializes the whole relay
/// behind that latency — which measured about 27 ms per message, roughly 37 messages/sec, regardless of
/// how fast anything else was. Handing the transport the whole batch lets it put every publish in flight
/// first and then collect the confirms, turning N round-trips into approximately one.
/// </para>
/// <para>
/// Implementations must not report a message confirmed until it is durably accepted; the relay marks rows
/// processed purely on the strength of that promise.
/// </para>
/// </summary>
public interface IMessagePublisher
{
    Task<IReadOnlyList<PublishOutcome>> PublishAsync(
        IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken);
}
