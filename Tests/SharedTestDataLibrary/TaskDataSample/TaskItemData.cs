using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;

namespace SharedTestDataLibrary.TaskDataSample;

public class TaskItemData
{
    public static TaskItemFormRequest CreateCreateTaskItemRequestEmpty()
    {
        return new TaskItemFormRequest() { Title = string.Empty, Description = string.Empty, TaskListId = 0 };
    }

    public static TaskItemFormRequest CreateCreateTaskItemRequestValid(int taskListId)
    {
        return CreateCreateTaskItemRequestEmpty() with { Title = "MockItemTitle", TaskListId = taskListId };
    }

    public static TaskItemFormRequest CreateCreateTaskItemRequestGenerator(int taskListId, int mockId)
    {
        return CreateCreateTaskItemRequestEmpty() with { Title = $"MockItemTitle {mockId}", TaskListId = taskListId };
    }

    public static TaskItemFormRequest CreateUpdateTaskItemRequestValid(string title, string description)
    {
        return new TaskItemFormRequest() { Id = 1, Title = title, Description = description, TaskListId = 0 };
    }
}
