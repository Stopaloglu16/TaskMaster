using Application.Messaging;
using Application.Messaging.Contracts;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ServiceLayer.FileJobs.Saga;

/// <summary>
/// The FileJob import saga's orchestrator. Every reply event advances one job's state and enqueues
/// the next command, both in the transaction MessageDispatcher opens — so a transition can never be
/// durable without the command that follows it, or vice versa.
/// <para>
/// Each handler returns silently when the job is not in the state it expects. Over a broker that
/// looks like it would discard an early reply, but it cannot: a command is only ever enqueued in the
/// same transaction as the transition preceding it, so the state is durably in place before the
/// command is publishable, and therefore before any reply to it can exist. An unexpected state is
/// always a redelivery of something already applied.
/// </para>
/// </summary>
public sealed class FileJobSagaHandler(ApplicationDbContext db, ILogger<FileJobSagaHandler> logger) :
    IMessageHandler<FileValidated>,
    IMessageHandler<FileValidationFailed>,
    IMessageHandler<RowsProcessed>,
    IMessageHandler<RowsProcessingFailed>,
    IMessageHandler<PromotedToLive>,
    IMessageHandler<PromotionFailed>,
    IMessageHandler<PromotionRolledBack>
{
    // Step 1 reply: rows validated -> resolve assignees.
    public async Task HandleAsync(FileValidated message, CancellationToken cancellationToken)
    {
        var job = await FindAsync(message.FileJobId, cancellationToken);
        if (job is null || job.State != SagaState.Started) { return; }

        logger.LogInformation("FileJob {FileJobId}: {Valid} valid, {Invalid} invalid row(s).",
            job.Id, message.ValidCount, message.InvalidCount);

        Transition(job, SagaState.Validated);
        db.Enqueue(new ProcessRows(Guid.NewGuid(), job.CorrelationId, job.Id));
    }

    // Step 1 failure: nothing has been written outside the job's own rows, so no compensation.
    public async Task HandleAsync(FileValidationFailed message, CancellationToken cancellationToken)
    {
        var job = await FindAsync(message.FileJobId, cancellationToken);
        if (job is null || job.State != SagaState.Started) { return; }

        Fail(job, message.Reason);
        Transition(job, SagaState.Cancelled);
    }

    // Step 2 reply: assignees resolved -> promote into TaskLists/TaskItems.
    public async Task HandleAsync(RowsProcessed message, CancellationToken cancellationToken)
    {
        var job = await FindAsync(message.FileJobId, cancellationToken);
        if (job is null || job.State != SagaState.Validated) { return; }

        Transition(job, SagaState.Processed);
        db.Enqueue(new PromoteToLive(Guid.NewGuid(), job.CorrelationId, job.Id));
    }

    // Step 2 failure: still nothing promoted, so no compensation.
    public async Task HandleAsync(RowsProcessingFailed message, CancellationToken cancellationToken)
    {
        var job = await FindAsync(message.FileJobId, cancellationToken);
        if (job is null || job.State != SagaState.Validated) { return; }

        Fail(job, message.Reason);
        Transition(job, SagaState.Cancelled);
    }

    // Step 3 reply: promotion confirmed. Only NOW is the job complete — the old code set this before
    // promoting, so a crash in between left a job claiming success with nothing promoted.
    public async Task HandleAsync(PromotedToLive message, CancellationToken cancellationToken)
    {
        var job = await FindAsync(message.FileJobId, cancellationToken);
        if (job is null || job.State != SagaState.Processed) { return; }

        job.IsCompleted = true;
        Transition(job, SagaState.Completed);

        logger.LogInformation("FileJob {FileJobId}: promoted {Count} task list(s).",
            job.Id, message.TaskListCount);
    }

    // Step 3 failure: promotion may have written some TaskLists before failing, so compensate.
    public async Task HandleAsync(PromotionFailed message, CancellationToken cancellationToken)
    {
        var job = await FindAsync(message.FileJobId, cancellationToken);
        if (job is null || job.State != SagaState.Processed) { return; }

        Fail(job, message.Reason);
        Transition(job, SagaState.Compensating);
        db.Enqueue(new RollbackPromotion(Guid.NewGuid(), job.CorrelationId, job.Id));
    }

    // Compensation reply: the partial promotion has been undone; the saga ends cancelled.
    public async Task HandleAsync(PromotionRolledBack message, CancellationToken cancellationToken)
    {
        var job = await FindAsync(message.FileJobId, cancellationToken);
        if (job is null || job.State != SagaState.Compensating) { return; }

        Transition(job, SagaState.Cancelled);
    }

    private async Task<FileJob?> FindAsync(int fileJobId, CancellationToken cancellationToken)
        => await db.FileJobs.FirstOrDefaultAsync(f => f.Id == fileJobId, cancellationToken);

    private void Transition(FileJob job, SagaState to)
    {
        logger.LogInformation("FileJob {FileJobId}: {From} -> {To}", job.Id, job.State, to);
        job.State = to;
        job.Version++; // compare-and-swap against concurrent replies for this same job
    }

    private static void Fail(FileJob job, string reason) => job.FailureReason = Truncate(reason, 1000);

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];
}
