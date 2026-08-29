using Application.Messaging;

namespace Infrastructure.Messaging;

/// <summary>
/// Maps message type names to CLR types. Populated at startup by <c>AddMessageHandler</c> so the
/// in-process bus knows how to deserialize each outbox payload back to its message type.
/// </summary>
public sealed class MessageTypeRegistry
{
    private readonly Dictionary<string, Type> _byName = new(StringComparer.Ordinal);

    public void Register(Type messageType)
    {
        if (!typeof(IMessage).IsAssignableFrom(messageType))
        {
            throw new ArgumentException($"{messageType.Name} does not implement {nameof(IMessage)}.", nameof(messageType));
        }

        _byName[messageType.Name] = messageType;
    }

    public Type? Resolve(string typeName) => _byName.GetValueOrDefault(typeName);
}
