using System.ComponentModel.DataAnnotations;
using System.Text;
using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Messaging;
using Application.Messaging.Contracts;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ServiceLayer.FileJobs.Saga;

/// <summary>
/// Saga step 1: DataAnnotations-validate every uploaded row and stamp it Validated or ValidateIssue.
/// <para>
/// The old <c>FileJobsService.ValidateFileJob</c> did one SaveChanges per row with no transaction and
/// swallowed the exception, so a crash left a half-validated file with no way to tell. Here every row
/// is mutated in the tracked graph and the dispatcher commits the lot once.
/// </para>
/// </summary>
public sealed class ValidateFileHandler(ApplicationDbContext db, ILogger<ValidateFileHandler> logger)
    : IMessageHandler<ValidateFile>
{
    public async Task HandleAsync(ValidateFile message, CancellationToken cancellationToken)
    {
        var rows = await db.FileJobUploads
                           .Where(row => row.FileJobId == message.FileJobId)
                           .ToListAsync(cancellationToken);

        // A business failure, not an infrastructure one: retrying will not conjure rows. Enqueue the
        // failure event so the saga cancels, rather than throwing onto the retry ladder.
        if (rows.Count == 0)
        {
            db.Enqueue(new FileValidationFailed(
                Guid.NewGuid(), message.CorrelationId, message.FileJobId, "The upload contains no rows."));
            return;
        }

        var valid = 0;
        var invalid = 0;

        foreach (var row in rows)
        {
            var errors = new StringBuilder();

            Validate(new CreateTaskListRequest
            {
                Title = row.TaskTitle,
                DueDate = row.DueDate,
                AssignedTo = row.AssignedTo,
            }, errors);

            // The original `continue`d here on a task-list failure, so those rows never got
            // ValidateIssue or an ErrorMessage written and silently stayed NewUpload. Validating the
            // item part too means one row reports every problem it has.
            Validate(new CreateTaskItemRequest
            {
                Title = row.Title,
                Description = row.Description,
            }, errors);

            if (errors.Length == 0)
            {
                row.FileRowType = FileRowStatus.Validated;
                row.ErrorMessage = null;
                valid++;
            }
            else
            {
                row.FileRowType = FileRowStatus.ValidateIssue;
                row.ErrorMessage = Truncate(errors.ToString(), 350); // column is varchar(350)
                invalid++;
            }
        }

        logger.LogInformation("FileJob {FileJobId}: validated {Valid} row(s), {Invalid} with issues.",
            message.FileJobId, valid, invalid);

        if (valid == 0)
        {
            db.Enqueue(new FileValidationFailed(
                Guid.NewGuid(), message.CorrelationId, message.FileJobId,
                $"All {invalid} row(s) failed validation."));
            return;
        }

        db.Enqueue(new FileValidated(Guid.NewGuid(), message.CorrelationId, message.FileJobId, valid, invalid));
    }

    private static void Validate(object request, StringBuilder errors)
    {
        var results = new List<ValidationResult>();

        if (Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true))
        {
            return;
        }

        foreach (var result in results)
        {
            errors.Append(result.ErrorMessage).Append(' ');
        }
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];
}
