namespace Infrastructure.Messaging;

/// <summary>
/// A message waiting to be published to the broker. Written to the database in the SAME transaction
/// as the business change that produced it (the outbox pattern), then relayed to RabbitMQ by
/// <see cref="OutboxRelay{TDbContext}"/>.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Monotonic insert order, assigned by a Postgres identity column, and the primary key.
    /// This — not <see cref="OccurredOn"/> — is what the relay orders by: wall-clock timestamps tie
    /// under load and can go backwards, which would let a saga's steps be published out of order.
    /// </summary>
    public long Sequence { get; set; }

    /// <summary>
    /// The message's <c>MessageId</c>. Travels to the broker as the AMQP message-id and becomes the
    /// receiver's inbox idempotency key. Uniquely indexed, so enqueuing the same id twice fails loudly
    /// instead of silently producing two deliveries.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>Message type name. Doubles as the topic-exchange routing key.</summary>
    public string Type { get; set; } = default!;

    /// <summary>JSON-serialized message body.</summary>
    public string Payload { get; set; } = default!;

    /// <summary>Saga correlation id. Published as the AMQP correlation-id for tracing.</summary>
    public Guid CorrelationId { get; set; }

    public DateTime OccurredOn { get; set; }

    /// <summary>
    /// Null until the broker has <em>confirmed</em> the publish. Set from a confirmed ack only —
    /// marking it as soon as the client library accepts the frame would lose messages whenever the
    /// broker dropped them (no route, disk full, restart mid-flight).
    /// </summary>
    public DateTime? ProcessedOn { get; set; }

    /// <summary>Publish attempts so far. Incremented by the relay when a publish fails.</summary>
    public int Attempts { get; set; }

    /// <summary>Why the last publish attempt failed. Left in place as a diagnostic breadcrumb.</summary>
    public string? LastError { get; set; }
}
