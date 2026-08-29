namespace Application.Messaging;

/// <summary>
/// Handles one message type. Implementations make their business change and call
/// <c>Enqueue</c> for any outgoing messages, but must NOT call <c>SaveChanges</c> or manage the
/// transaction — the dispatcher wraps the call in a transaction (with the inbox insert) and commits
/// once the handler returns.
/// </summary>
public interface IMessageHandler<in TMessage> where TMessage : IMessage
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken);
}
