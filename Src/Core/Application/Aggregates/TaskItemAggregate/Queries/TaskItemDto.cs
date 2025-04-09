namespace Application.Aggregates.TaskItemAggregate.Queries;

public record TaskItemDto
{
    public int Id { get; init; }
    public string Title { get; init; }
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
    public DateOnly? CompletedDate { get; init; }
}
