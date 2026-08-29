using Application.Messaging;
using Application.Messaging.Contracts;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ServiceLayer.FileJobs.Saga;

/// <summary>
/// Saga step 2: resolve each validated row's <c>AssignedTo</c> name to a user id.
/// <para>
/// The old worker did this ten rows at a time with a 2-4 second artificial delay per row and a user
/// list snapshotted once per outer loop (so it went stale on long jobs). This reads the users once
/// per message and processes the whole job in one transaction.
/// </para>
/// </summary>
public sealed class ProcessRowsHandler(ApplicationDbContext db, ILogger<ProcessRowsHandler> logger)
    : IMessageHandler<ProcessRows>
{
    public async Task HandleAsync(ProcessRows message, CancellationToken cancellationToken)
    {
        var rows = await db.FileJobUploads
                           .Where(row => row.FileJobId == message.FileJobId &&
                                         row.FileRowType == FileRowStatus.Validated)
                           .ToListAsync(cancellationToken);

        var usersByName = await db.Users
            .Where(user => user.UserTypeId == UserType.TaskUser)
            .ToDictionaryAsync(user => user.FullName, user => user.Id, cancellationToken);

        var processed = 0;

        foreach (var row in rows)
        {
            if (string.IsNullOrEmpty(row.AssignedTo))
            {
                row.AssignedToId = null;
                row.FileRowType = FileRowStatus.Processed;
                processed++;
                continue;
            }

            if (usersByName.TryGetValue(row.AssignedTo, out var userId))
            {
                row.AssignedToId = userId;
                row.FileRowType = FileRowStatus.Processed;
                processed++;
            }
            else
            {
                row.FileRowType = FileRowStatus.ProcessIssue;
                row.ErrorMessage = Truncate($"AssignedTo user '{row.AssignedTo}' not found.", 350);
            }
        }

        logger.LogInformation("FileJob {FileJobId}: processed {Processed} of {Total} row(s).",
            message.FileJobId, processed, rows.Count);

        // Every row named an assignee that does not exist. Retrying cannot fix that, so fail the
        // saga rather than throwing onto the retry ladder.
        if (processed == 0)
        {
            db.Enqueue(new RowsProcessingFailed(
                Guid.NewGuid(), message.CorrelationId, message.FileJobId,
                $"None of the {rows.Count} validated row(s) could be assigned to a user."));
            return;
        }

        db.Enqueue(new RowsProcessed(Guid.NewGuid(), message.CorrelationId, message.FileJobId, processed));
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];
}
