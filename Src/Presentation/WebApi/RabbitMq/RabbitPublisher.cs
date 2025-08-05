using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace WebApi.RabbitMq;

public class RabbitPublisher : IDisposable
{
    private readonly IConnection _conn;
    private readonly IModel _channel;
    private readonly string _queue = "catalogEvents";


    public RabbitPublisher(IConnection conn)
    {
        _conn = conn;
        _channel = conn.CreateModel();
        _channel.QueueDeclare(_queue, durable: false, exclusive: false, autoDelete: false);
    }

    public void Publish(ProcessMessage msg)
    {
        
        try
        {
            var publishhh = _conn.Endpoint;


            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            _channel.BasicPublish( "", _queue, body: body);

        }
        catch (Exception ex)
        {

            throw;
        }

    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
