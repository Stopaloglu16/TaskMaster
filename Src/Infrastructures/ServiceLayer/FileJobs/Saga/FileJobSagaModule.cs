using Application.Messaging.Contracts;
using Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceLayer.FileJobs.Saga;

public static class FileJobSagaModule
{
    /// <summary>
    /// The orchestrator's consumer group: every reply event that advances a FileJob's state.
    /// <para>
    /// Prefetch is deliberately lower than the worker group's. Every message on this queue contends
    /// for the same FileJob row, so extra concurrency here mostly buys optimistic-concurrency
    /// conflicts and retries rather than throughput.
    /// </para>
    /// </summary>
    public static IServiceCollection AddFileJobSagaModule(this IServiceCollection services) =>
        services.AddConsumer("filejob-saga", c =>
        {
            c.Prefetch = 8;
            c.Handles<FileJobSagaHandler, FileValidated>()
             .Handles<FileJobSagaHandler, FileValidationFailed>()
             .Handles<FileJobSagaHandler, RowsProcessed>()
             .Handles<FileJobSagaHandler, RowsProcessingFailed>()
             .Handles<FileJobSagaHandler, PromotedToLive>()
             .Handles<FileJobSagaHandler, PromotionFailed>()
             .Handles<FileJobSagaHandler, PromotionRolledBack>();
        });

    /// <summary>
    /// The participants' consumer group: the commands that actually do the import work. These touch
    /// different jobs' rows rather than one shared saga row, so they can run wider.
    /// </summary>
    public static IServiceCollection AddFileJobWorkerModule(this IServiceCollection services) =>
        services.AddConsumer("filejob-worker", c => c
            .Handles<ValidateFileHandler, ValidateFile>()
            .Handles<ProcessRowsHandler, ProcessRows>()
            .Handles<PromoteToLiveHandler, PromoteToLive>()
            .Handles<RollbackPromotionHandler, RollbackPromotion>());
}
