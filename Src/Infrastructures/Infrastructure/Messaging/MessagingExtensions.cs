using Infrastructure.Messaging.InProcess;
using Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure.Messaging;

/// <summary>Which transport the outbox relay publishes through.</summary>
public enum MessageTransport
{
    /// <summary>Durable AMQP delivery with retry ladder and dead-letter queues.</summary>
    RabbitMq,

    /// <summary>Broker-free inline dispatch. Benchmark baseline only — see <see cref="InProcessPublisher{T}"/>.</summary>
    InProcess,
}

public static class MessagingExtensions
{
    /// <summary>
    /// The configured transport, defaulting to RabbitMQ. Read by the host too, which has to decide
    /// whether to register a broker connection at all.
    /// </summary>
    public static MessageTransport GetMessageTransport(this IConfiguration configuration) =>
        configuration.GetValue<MessageTransport?>("Messaging:Transport") ?? MessageTransport.RabbitMq;

    /// <summary>
    /// Registers the transport-agnostic messaging core: the type and consumer-group registries and the
    /// dispatcher that owns each handler's transaction. Call this BEFORE any
    /// <see cref="AddConsumer"/>, which needs the registries to exist.
    /// <para>
    /// A host that only <em>produces</em> messages (the API, which just writes outbox rows) does not
    /// need any of this — <c>Enqueue</c> is a plain DbContext call.
    /// </para>
    /// </summary>
    public static IServiceCollection AddMessagingCore<TDbContext>(this IServiceCollection services)
        where TDbContext : OutboxDbContext
    {
        services.TryAddSingleton(new MessageTypeRegistry());
        services.TryAddSingleton(new ConsumerRegistry());
        services.TryAddSingleton<MessageDispatcher<TDbContext>>();
        return services;
    }

    /// <summary>
    /// Declares a consumer group — one durable queue, one inbox discriminator, one unit of scaling —
    /// and the handlers bound to it. This is a module's entire subscription surface; the broker
    /// topology is derived from these registrations at startup.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddConsumer("inventory", c => c
    ///     .Handles&lt;ReserveStockHandler, ReserveStock&gt;()
    ///     .Handles&lt;ReleaseStockHandler, ReleaseStock&gt;());
    /// </code>
    /// </example>
    public static IServiceCollection AddConsumer(
        this IServiceCollection services, string name, Action<ConsumerBuilder> configure)
    {
        var types = services.Registry<MessageTypeRegistry>();
        var consumers = services.Registry<ConsumerRegistry>();

        var builder = new ConsumerBuilder(name, services, types);
        configure(builder);
        consumers.Add(builder.Build());

        return services;
    }

    /// <summary>
    /// Registers the outbox relay plus the transport it publishes through, and — for RabbitMQ — the
    /// consumers. Registration order is load-bearing: the topology must be declared before the relay
    /// can publish to it or a consumer can subscribe, and <see cref="IHostedService"/>s are started in
    /// the order they are added.
    /// </summary>
    public static IServiceCollection AddMessageTransport<TDbContext>(
        this IServiceCollection services, IConfiguration configuration)
        where TDbContext : OutboxDbContext
    {
        if (configuration.GetMessageTransport() is MessageTransport.RabbitMq)
        {
            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
            services.AddSingleton<RabbitMqPublisher>();
            services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqPublisher>());

            services.AddHostedService<RabbitMqTopology>();
            services.AddHostedService<OutboxRelay<TDbContext>>();
            services.AddHostedService<RabbitMqConsumerService<TDbContext>>();
        }
        else
        {
            services.AddSingleton<IMessagePublisher, InProcessPublisher<TDbContext>>();
            services.AddHostedService<OutboxRelay<TDbContext>>();
        }

        return services;
    }

    /// <summary>
    /// Fetches a registry instance back out of the service collection at registration time. The
    /// registries are populated while the container is still being built, so they cannot be resolved
    /// from a provider yet.
    /// </summary>
    private static T Registry<T>(this IServiceCollection services) where T : class =>
        services.FirstOrDefault(d => d.ServiceType == typeof(T))?.ImplementationInstance as T
        ?? throw new InvalidOperationException(
            $"Call {nameof(AddMessagingCore)} before {nameof(AddConsumer)}.");
}
