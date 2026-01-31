using System;
using Microsoft.Extensions.Logging;

namespace WebApiEmailService.Services
{
    public class JobTestService : IJobTestService
    {
        private readonly ILogger _logger;

        public JobTestService(ILogger<JobTestService> logger)
        {
            _logger = logger;
        }

        public void FireAndForgetJob()
        {
            // Fire-and-forget jobs are queued and execute in the background once.
            _logger.LogInformation(
                "FireAndForgetJob queued. Description: Executes once in background without waiting for completion. QueuedAt: {QueuedAt}.",
                DateTimeOffset.UtcNow);
        }

        public void DelayedJob()
        {
            // Delayed jobs are scheduled to run after a specified delay.
            // Note: actual scheduling/delay is handled by the caller/scheduler; this method logs execution metadata.
            _logger.LogInformation(
                "DelayedJob executed. Description: Runs after a configured delay. ExecutedAt: {ExecutedAt}.",
                DateTimeOffset.UtcNow);
        }

        public void RecurringJob()
        {
            // Recurring jobs run on a configured schedule (e.g., via CRON).
            _logger.LogInformation(
                "RecurringJob executed. Description: Runs on configured schedule (CRON). ExecutedAt: {ExecutedAt}.",
                DateTimeOffset.UtcNow);
        }

        public void ContinuationJob()
        {
            // Continuation jobs run after a parent job completes.
            _logger.LogInformation(
                "ContinuationJob started. Description: Runs as a continuation after a parent job completes. StartedAt: {StartedAt}.",
                DateTimeOffset.UtcNow);
        }




    }
}
