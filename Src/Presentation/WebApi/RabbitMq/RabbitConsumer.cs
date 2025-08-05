using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace WebApi.RabbitMq
{

    public class RabbitConsumer : BackgroundService
    {
    
        private readonly ILogger<RabbitConsumer> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _messageConnection;
        private IModel? _messageChannel;


        public RabbitConsumer(ILogger<RabbitConsumer> logger, IConfiguration config, IServiceProvider serviceProvider, IConnection? messageConnection)
        {
            _logger = logger;
            _config = config;
            _serviceProvider = serviceProvider;
        }


        protected override Task ExecuteAsync(CancellationToken ct)
        {
            string queueName = "catalogEvents";

            _messageConnection = _serviceProvider.GetService<IConnection>();

            _messageChannel = _messageConnection!.CreateModel();
            _messageChannel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_messageChannel);
            consumer.Received += OnReceivedAsync;

            _messageChannel.BasicConsume(queue: queueName,
                autoAck: true,
                consumer: consumer);

            return Task.CompletedTask;


            //// 1. open a long‑lived channel from the injected connection
            //_channel = _conn.CreateModel();

            //var consumee = _conn.Endpoint;

            

            //// 2. declare the queue (idempotent)
            //_channel.QueueDeclare("processQ", durable: false, exclusive: false, autoDelete: false);

            //// 3. create and wire the consumer
            //var consumer = new AsyncEventingBasicConsumer(_channel);
            //consumer.Received += OnReceivedAsync; // attach handler FIRST

            //// 4. subscribe to the queue
            //_channel.BasicConsume(queue: "processQ",
            //                      autoAck: false,
            //                      consumer: consumer);

            //// 5. keep the method alive until cancellation is requested
            ////ct.Register(() =>
            ////{
            ////    _channel?.Close();
            ////    _channel?.Dispose();
            ////});

            //while (!ct.IsCancellationRequested)
            //{
            //    await Task.Delay(1000, ct);
            //}
        }

        private void OnReceivedAsync(object sender, BasicDeliverEventArgs args)
        {
           
            try
            {
                string messagetext = Encoding.UTF8.GetString(args.Body.ToArray());
                _logger.LogInformation("All products retrieved from the catalog at {now}. Message Text: {text}", DateTime.Now, messagetext);

                var message = args.Body;

                
                //var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                //var msg = JsonSerializer.Deserialize<ProcessMessage>(json)!;

                //// do work...
                //await Task.Delay(1000);

                //_store.Add(msg.RequestId,
                //           new ProcessResult(msg.Item.Id, "Done", $"Processed {msg.Item.Name}"));

                //// ACK the message
                //var channel = ((AsyncEventingBasicConsumer)sender).Model;
                //channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                // Optionally: NACK the message or handle error
            }
        }

     
    }
}
