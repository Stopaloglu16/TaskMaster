namespace WebApiEmailService.Services
{
    /// <summary>
    ///  To test hangfire job types
    /// </summary>
    public interface IJobTestService
    {
        void FireAndForgetJob();
        void DelayedJob();
        void RecurringJob();
        void ContinuationJob();
    }
}
