namespace Application.Aggregates.TaskItemAggregate.Commands.Update;

public record CompleteTaskItemRequest(int taskListId, int taskItemId);
