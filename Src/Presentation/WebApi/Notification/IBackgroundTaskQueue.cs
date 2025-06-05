using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Microsoft.AspNetCore.SignalR;
using ServiceLayer.TaskLists;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace WebApi.Notification
{
    //public interface IBackgroundTaskQueue
    //{
    //    ValueTask EnqueueAsync(WorkItem workItem);
    //    ValueTask<WorkItem> DequeueAsync(CancellationToken cancellationToken);
    //}

    public interface IBackgroundTaskQueue
    {
        void QueueTask(string taskId, IEnumerable<CreateTaskListRequest> requests);
        bool TryDequeue(out (string TaskId, IEnumerable<CreateTaskListRequest> Requests) task);
    }


  
}
