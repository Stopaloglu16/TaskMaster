using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceLayer.TaskLists;
using System.Text;
using System.Text.Json;
using WebApi.Notification;

namespace WebApi.RabbitMq
{

    public class RabbitConsumer : BackgroundService
    {

        private readonly ILogger<RabbitConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ResultStore _store;
        private readonly IHubContext<TaskProgressHub> _hubContext;
        private IConnection? _messageConnection;
        private IChannel? _channel;


        public RabbitConsumer(ILogger<RabbitConsumer> logger,
                              IServiceProvider serviceProvider,
                              ResultStore store,
                              IHubContext<TaskProgressHub> hubContext)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _store = store;
            _hubContext = hubContext;
        }



        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _messageConnection = _serviceProvider.GetRequiredService<IConnection>();

            _channel = await _messageConnection.CreateChannelAsync(cancellationToken: ct);

            await _channel.QueueDeclareAsync(queue: RabbitQueues.TaskListBulk,
                durable: RabbitQueues.Durable,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct);

            // One batch at a time per consumer, so a slow bulk insert does not hoard the queue.
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                    _logger.LogInformation("Message received: {message}", message);

                    await HandleMessageAsync(message, ct);

                    // ACK when processing is successful
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");

                    // NACK without requeue - a poison batch would otherwise spin forever.
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: ct);
                }
            };

            // Start consuming
            await _channel.BasicConsumeAsync(
                queue: RabbitQueues.TaskListBulk,
                autoAck: false,
                consumer: consumer,
                cancellationToken: ct);

            // Keep the service alive; the consumer runs on the connection's own dispatcher.
            await Task.Delay(Timeout.Infinite, ct).ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnCanceled);
        }

        private async Task HandleMessageAsync(string message, CancellationToken ct)
        {
            var msg = JsonSerializer.Deserialize<TaskListBulkMessage>(message,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            if (msg is null || msg.Items is null || msg.Items.Count == 0)
            {
                _logger.LogWarning("Skipping empty or unreadable bulk message");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var taskListService = scope.ServiceProvider.GetRequiredService<ITaskListService>();

            var result = await taskListService.CreateTaskListBulk(msg.Items, ct);

            var responses = result.Value ?? new List<CreateTaskListResponse>();

            _store.Add(msg.RequestId, responses);

            // Same contract the in-process worker uses, so the page needs no new handler.
            await _hubContext.Clients.Group(msg.RequestId).SendAsync("TaskCompleted", msg.RequestId, responses, ct);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null)
                await _channel.DisposeAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}
