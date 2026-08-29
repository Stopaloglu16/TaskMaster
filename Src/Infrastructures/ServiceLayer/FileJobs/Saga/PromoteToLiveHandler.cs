using Application.Messaging;
using Application.Messaging.Contracts;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ServiceLayer.FileJobs.Saga;

/// <summary>
/// Saga step 3: promote the processed rows into real TaskLists and TaskItems.
/// <para>
/// This replaces the old <c>MoveToLiveAsync</c> raw SQL, which had four separate defects: it matched
/// <c>FileRowType = 2</c> (Validated) even though the worker had already moved good rows to 3
/// (Processed), so it promoted nothing in the normal path; its old-id-to-new-id CTE joined on
/// Title + DueDate + AssignedToId and fanned out into a cross product whenever two rows shared them;
/// that join broke entirely on a NULL AssignedToId; and it ran outside any transaction shared with
/// the completion write. Grouping in memory and letting EF assign the foreign keys removes all four.
/// </para>
/// </summary>
public sealed class PromoteToLiveHandler(ApplicationDbContext db, ILogger<PromoteToLiveHandler> logger)
    : IMessageHandler<PromoteToLive>
{
    public async Task HandleAsync(PromoteToLive message, CancellationToken cancellationToken)
    {
        // Idempotency belt-and-braces: the inbox already dedupes, but a partially-promoted job that
        // got here again must not double-insert.
        var alreadyPromoted = await db.TaskLists
            .IgnoreQueryFilters()
            .AnyAsync(list => list.SourceFileJobId == message.FileJobId, cancellationToken);

        if (alreadyPromoted)
        {
            logger.LogInformation("FileJob {FileJobId}: already promoted; skipping.", message.FileJobId);
            db.Enqueue(new PromotedToLive(Guid.NewGuid(), message.CorrelationId, message.FileJobId, 0));
            return;
        }

        var rows = await db.FileJobUploads
                           .Where(row => row.FileJobId == message.FileJobId &&
                                         row.FileRowType == FileRowStatus.Processed)
                           .OrderBy(row => row.Id)
                           .ToListAsync(cancellationToken);

        // One TaskList per distinct (title, due date, assignee); every row under it becomes an item.
        var groups = rows.GroupBy(row => new { row.TaskTitle, row.DueDate, row.AssignedToId }).ToList();

        // TaskItemConfiguration puts a unique index on (TaskListId, Title), so duplicate item titles
        // inside one list would abort the whole transaction on SaveChanges. Catch it here instead:
        // this is a business problem with the CSV that no amount of retrying will fix, so fail the
        // saga cleanly rather than letting it throw onto the retry ladder and end in the DLQ.
        var duplicate = groups
            .SelectMany(group => group.GroupBy(row => row.Title)
                                      .Where(byTitle => byTitle.Count() > 1)
                                      .Select(byTitle => new { group.Key.TaskTitle, Title = byTitle.Key }))
            .FirstOrDefault();

        if (duplicate is not null)
        {
            db.Enqueue(new PromotionFailed(
                Guid.NewGuid(), message.CorrelationId, message.FileJobId,
                $"Task list '{duplicate.TaskTitle}' contains more than one item titled '{duplicate.Title}'."));
            return;
        }

        var taskListCount = 0;

        foreach (var group in groups)
        {
            var taskList = new TaskList
            {
                Title = group.Key.TaskTitle,
                DueDate = group.Key.DueDate,
                AssignedToId = group.Key.AssignedToId,
                IsCompleted = false,
                CompletedDate = null,
                SourceFileJobId = message.FileJobId,
            };

            foreach (var row in group)
            {
                taskList.TaskItems.Add(new TaskItem
                {
                    Title = row.Title,
                    Description = row.Description,
                    IsCompleted = false,
                    CompletedDate = null,
                });

                row.FileRowType = FileRowStatus.MovedToLive;
            }

            db.TaskLists.Add(taskList);
            taskListCount++;
        }

        logger.LogInformation("FileJob {FileJobId}: promoting {Lists} task list(s) from {Rows} row(s).",
            message.FileJobId, taskListCount, rows.Count);

        db.Enqueue(new PromotedToLive(Guid.NewGuid(), message.CorrelationId, message.FileJobId, taskListCount));
    }
}
