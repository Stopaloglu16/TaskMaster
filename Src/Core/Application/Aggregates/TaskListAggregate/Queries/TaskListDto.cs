namespace Application.Aggregates.TaskListAggregate.Queries;

public record TaskListDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public DateOnly DueDate { get; init; }
    public DateOnly? CompletedDate { get; init; }
    public string? AssignedTo { get; set; }

    public int TaskItemCount { get; init; } = 0;
    public int TaskItemCompletedCount { get; init; } = 0;
}
