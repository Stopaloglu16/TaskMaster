using Application.Aggregates.TaskItemAggregate.Commands.Create;
using Application.Aggregates.TaskItemAggregate.Commands.Update;

namespace SharedTestDataLibrary.TaskDataSample;

public class TaskItemData
{
    public static CreateTaskItemRequest CreateCreateTaskItemRequestEmpty()
    {
        return new CreateTaskItemRequest() { Title = string.Empty, Description = string.Empty };
    }

    public static CreateTaskItemRequest CreateCreateTaskItemRequestValid(int taskListId)
    {
        return CreateCreateTaskItemRequestEmpty() with { Title = "MockItemTitle", TaskListId = taskListId };
    }

    public static CreateTaskItemRequest CreateCreateTaskItemRequestGenerator(int taskListId, int mockId)
    {
        return CreateCreateTaskItemRequestEmpty() with { Title = $"MockItemTitle {mockId}", TaskListId = taskListId };
    }

    public static UpdateTaskItemRequest CreateUpdateTaskItemRequestValid(string title, string description)
    {
        return new UpdateTaskItemRequest() { Id = 1, Title = title, Description = description };
    }
}
