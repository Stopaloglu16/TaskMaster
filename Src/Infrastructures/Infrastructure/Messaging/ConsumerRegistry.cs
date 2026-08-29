using Application.Messaging;

namespace Infrastructure.Messaging;

/// <summary>
/// One consumer group: a durable queue, the message types bound to it, and how many deliveries it
/// will work on at once. A group is the unit of independent, scalable consumption — in this codebase
/// there is one per module, which is also the value written to <see cref="InboxMessage.Consumer"/>.
/// </summary>
public sealed class ConsumerGroup(string name, IReadOnlyList<Type> messageTypes, ushort prefetch)
{
    /// <summary>Queue name and inbox discriminator. Also the retry/dead-letter routing key.</summary>
    public string Name { get; } = name;

    /// <summary>Message types bound to this group's queue. Each becomes a routing-key binding.</summary>
    public IReadOnlyList<Type> MessageTypes { get; } = messageTypes;

    /// <summary>
    /// Unacked-delivery window (AMQP basic.qos). This is the consumer's concurrency and its
    /// backpressure: the broker will not hand this group more than this many messages at once.
    /// </summary>
    public ushort Prefetch { get; } = prefetch;
}

/// <summary>Fluent surface for <c>AddConsumer</c>. See <see cref="MessagingExtensions.AddConsumer"/>.</summary>
public sealed class ConsumerBuilder(string name, IServiceCollection services, MessageTypeRegistry types)
{
    private readonly List<Type> _messageTypes = [];

    /// <summary>How many deliveries this group processes concurrently. Default 16.</summary>
    public ushort Prefetch { get; set; } = 16;

    /// <summary>
    /// Bind <typeparamref name="TMessage"/> to this group's queue and register the handler that
    /// processes it. Exactly one handler per message type per group.
    /// </summary>
    public ConsumerBuilder Handles<THandler, TMessage>()
        where THandler : class, IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        services.AddScoped<IMessageHandler<TMessage>, THandler>();
        types.Register(typeof(TMessage));
        _messageTypes.Add(typeof(TMessage));
        return this;
    }

    internal ConsumerGroup Build() => new(name, _messageTypes, Prefetch);
}

/// <summary>
/// Every consumer group declared at startup. The RabbitMQ topology (queues, bindings, retry and
/// dead-letter plumbing) is derived entirely from this — declaring a group is the only thing a
/// module has to do to get a queue.
/// </summary>
public sealed class ConsumerRegistry
{
    private readonly List<ConsumerGroup> _groups = [];

    public IReadOnlyList<ConsumerGroup> Groups => _groups;

    internal void Add(ConsumerGroup group)
    {
        if (_groups.Any(g => g.Name == group.Name))
        {
            throw new InvalidOperationException($"Consumer group '{group.Name}' is already registered.");
        }

        _groups.Add(group);
    }

    /// <summary>
    /// Groups subscribed to a message type name. Used by the in-process transport to work out where a
    /// message would have gone; the RabbitMQ transport lets the broker's bindings answer this instead.
    /// </summary>
    public IEnumerable<ConsumerGroup> GroupsFor(string typeName) =>
        _groups.Where(g => g.MessageTypes.Any(t => t.Name == typeName));
}
