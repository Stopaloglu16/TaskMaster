using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace WebApi.RabbitMq;

public class RabbitPublisher : IDisposable
{
    private readonly IConnection _conn;
    private readonly IChannel _channel;
    private readonly string _queue = "catalogEvents";

    private RabbitPublisher(IConnection conn, IChannel channel)
    {
        _conn = conn;
        _channel = channel;
    }

    public static async Task<RabbitPublisher> CreateAsync(IConnection conn)
    {
        if (conn == null)
            throw new ArgumentNullException(nameof(conn));

        // Create channel using async API
        var channel = await conn.CreateChannelAsync();

        // Ensure the queue exists (passive = false → declare if missing)
        await channel.QueueDeclareAsync(
            queue: "my-queue-name",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        return new RabbitPublisher(conn, channel);
    }

    public async Task Publish(ProcessMessage msg)
    {
        if (msg is null) throw new ArgumentNullException(nameof(msg));

        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            // new API expects RabbitMQ.Client.BasicProperties (class) for properties; null is acceptable
            await _channel.BasicPublishAsync(exchange: "", routingKey: _queue, body: body);
        }
        catch (Exception)
        {
            // rethrow to preserve stack trace
            throw;
        }
    }

    public void Dispose()
    {
        // IChannel implements IDisposable
        _channel?.Dispose();
    }
}
