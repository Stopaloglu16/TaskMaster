using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace WebApi.RabbitMq;

public class RabbitPublisher : IAsyncDisposable
{
    private readonly IConnection _conn;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IChannel? _channel;

    public RabbitPublisher(IConnection conn)
    {
        _conn = conn ?? throw new ArgumentNullException(nameof(conn));
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true }) return _channel;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true }) return _channel;

            var channel = await _conn.CreateChannelAsync(cancellationToken: cancellationToken);

            // Declare the queue we publish to - same name and durability as the consumer declares.
            await channel.QueueDeclareAsync(
                queue: RabbitQueues.TaskListBulk,
                durable: RabbitQueues.Durable,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            _channel = channel;
            return _channel;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task Publish(TaskListBulkMessage msg, CancellationToken cancellationToken = default)
    {
        if (msg is null) throw new ArgumentNullException(nameof(msg));

        var channel = await GetChannelAsync(cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

        var props = new BasicProperties { Persistent = RabbitQueues.Durable };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: RabbitQueues.TaskListBulk,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        _gate.Dispose();
        GC.SuppressFinalize(this);
    }
}
