using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;

namespace SharedTestDataLibrary.TaskDataSample;

public class TaskListData
{
    public static TaskListFormRequest CreateCreateTaskListRequestEmpty()
    {
        return new TaskListFormRequest() { Title = string.Empty, DueDate = DateOnly.FromDateTime(DateTime.Now) };
    }

    public static TaskListFormRequest CreateCreateTaskListRequestValid()
    {
        return CreateCreateTaskListRequestEmpty() with { Title = "MockTitle" };
    }

    public static TaskListFormRequest CreateUpdateTaskListRequestEmpty()
    {
        return new TaskListFormRequest() { Id = 1, Title = string.Empty, DueDate = DateOnly.FromDateTime(DateTime.Now) };
    }
}