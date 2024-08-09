namespace Domain.Exceptions;


public sealed class ReachedMaxTaskList : BadRequestException
{
    public ReachedMaxTaskList(string userName)
        : base($"The user {userName} reached to max task assign")
    {
    }
}
