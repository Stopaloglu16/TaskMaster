using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Microsoft.AspNetCore.SignalR;
using ServiceLayer.TaskLists;
using System.Collections.Concurrent;

namespace WebApi.Notification
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<(string, IEnumerable<CreateTaskListRequest>)> _queue = new();

        public void QueueTask(string taskId, IEnumerable<CreateTaskListRequest> requests)
            => _queue.Enqueue((taskId, requests));

        public bool TryDequeue(out (string TaskId, IEnumerable<CreateTaskListRequest> Requests) task)
            => _queue.TryDequeue(out task);

    }


    public class TaskProcessingWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IHubContext<TaskProgressHub> _hubContext;

        public TaskProcessingWorker(IServiceProvider serviceProvider,
                                    IBackgroundTaskQueue taskQueue,
                                    IHubContext<TaskProgressHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _taskQueue = taskQueue;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_taskQueue.TryDequeue(out var item))
                {
                    using var scope = _serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<ITaskListService>();
                    var result = await service.CreateTaskListBulk(item.Requests, stoppingToken);

                    // Notify client via SignalR
                    await _hubContext.Clients.Group(item.TaskId).SendAsync("TaskCompleted", item.TaskId, result.Value);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
