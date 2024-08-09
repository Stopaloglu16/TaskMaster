namespace Domain.Exceptions;

public sealed class ReachedMaxTaskItems : BadRequestException
{
    public ReachedMaxTaskItems(string taskListName)
        : base($"The task {taskListName} reached to max task")
    {
    }
}
