namespace Infrastructure.Messaging;

/// <summary>
/// Record of a message that a given consumer group has already processed. Inserted inside the same
/// transaction as the handler's work, so a redelivery is detected and skipped.
/// <para>
/// The key is <c>(MessageId, Consumer)</c>, not <c>MessageId</c> alone. With a topic exchange one
/// published message can fan out to several queues, and each consumer group must be allowed to
/// process it once — a single-column key would let the first group's inbox row suppress every other
/// group's handler. The old in-process bus got away with it only because every message type had
/// exactly one handler.
/// </para>
/// <para>
/// The composite primary key doubles as the race guard: if the same delivery is somehow handled
/// twice concurrently, both transactions insert this row and Postgres fails one with a unique
/// violation, which <see cref="MessageDispatcher{TDbContext}"/> reads as "duplicate".
/// </para>
/// </summary>
public class InboxMessage
{
    /// <summary>The consumed message's <c>MessageId</c>.</summary>
    public Guid MessageId { get; set; }

    /// <summary>The consumer group (= queue name) that processed it.</summary>
    public string Consumer { get; set; } = default!;

    public string Type { get; set; } = default!;

    public DateTime ReceivedOn { get; set; }
}
