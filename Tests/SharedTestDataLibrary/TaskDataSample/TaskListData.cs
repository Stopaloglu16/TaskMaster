using Application.Aggregates.TaskListAggregate.Commands.Create;
using Application.Aggregates.TaskListAggregate.Commands.Update;

namespace SharedTestDataLibrary.TaskDataSample;

public class TaskListData
{
    public static CreateTaskListRequest CreateCreateTaskListRequestEmpty()
    {
        return new CreateTaskListRequest() { Title = string.Empty, DueDate = DateOnly.FromDateTime(DateTime.Now) };
    }

    public static CreateTaskListRequest CreateCreateTaskListRequestValid()
    {
        return CreateCreateTaskListRequestEmpty() with { Title = "MockTitle" };
    }

    public static UpdateTaskListRequest CreateUpdateTaskListRequestEmpty()
    {
        return new UpdateTaskListRequest() { Id = 1, Title = string.Empty, DueDate = DateOnly.FromDateTime(DateTime.Now) };
    }

}
