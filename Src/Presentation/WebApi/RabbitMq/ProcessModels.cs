using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;

namespace WebApi.RabbitMq
{
    public class ProcessModels
    {
    }

    // Contracts/ProcessItem.cs
    public record ProcessItem(string Id, string Name);

    // Contracts/ProcessResult.cs
    public record ProcessResult(string Id, string Status, string Output);

    // Contracts/ProcessMessage.cs
    public record ProcessMessage(string RequestId, ProcessItem Item);

    /// <summary>
    /// The message the bulk-upload page actually puts on the queue: one batch of task lists
    /// plus the requestId the browser joined as a SignalR group.
    /// </summary>
    public record TaskListBulkMessage(string RequestId, List<CreateTaskListRequest> Items);
}
