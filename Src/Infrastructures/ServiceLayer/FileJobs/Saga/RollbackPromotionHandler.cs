using Application.Messaging;
using Application.Messaging.Contracts;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ServiceLayer.FileJobs.Saga;

/// <summary>
/// The saga's compensation: delete everything a failed promotion managed to create, so a cancelled
/// import leaves no half-promoted task lists behind.
/// <para>
/// This is why <c>TaskList.SourceFileJobId</c> exists — without recorded provenance there is no way
/// to tell which lists came from this job. The delete is a hard delete, not the usual soft delete:
/// these rows were never legitimately live, so leaving tombstones would be misleading. TaskItems go
/// with them via the cascade configured in TaskItemConfiguration.
/// </para>
/// </summary>
public sealed class RollbackPromotionHandler(ApplicationDbContext db, ILogger<RollbackPromotionHandler> logger)
    : IMessageHandler<RollbackPromotion>
{
    public async Task HandleAsync(RollbackPromotion message, CancellationToken cancellationToken)
    {
        var promoted = await db.TaskLists
            .IgnoreQueryFilters()
            .Include(list => list.TaskItems)
            .Where(list => list.SourceFileJobId == message.FileJobId)
            .ToListAsync(cancellationToken);

        if (promoted.Count > 0)
        {
            db.TaskLists.RemoveRange(promoted);
        }

        // Put the rows back to Processed so the job's row states match "nothing was promoted".
        var rows = await db.FileJobUploads
                           .Where(row => row.FileJobId == message.FileJobId &&
                                         row.FileRowType == FileRowStatus.MovedToLive)
                           .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            row.FileRowType = FileRowStatus.Processed;
        }

        logger.LogWarning("FileJob {FileJobId}: rolled back {Lists} task list(s) and {Rows} row(s).",
            message.FileJobId, promoted.Count, rows.Count);

        db.Enqueue(new PromotionRolledBack(Guid.NewGuid(), message.CorrelationId, message.FileJobId));
    }
}
