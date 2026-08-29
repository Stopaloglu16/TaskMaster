namespace Application.Messaging;

/// <summary>
/// Every message on the bus. <see cref="MessageId"/> is the receiving inbox's idempotency key;
/// <see cref="CorrelationId"/> is the saga instance the message belongs to (for a FileJob import,
/// <c>FileJob.CorrelationId</c>) and is published as the AMQP correlation-id for tracing.
/// </summary>
public interface IMessage
{
    Guid MessageId { get; }

    Guid CorrelationId { get; }
}
