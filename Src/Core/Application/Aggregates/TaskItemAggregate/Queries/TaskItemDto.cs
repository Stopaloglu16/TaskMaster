namespace Application.Aggregates.TaskItemAggregate.Queries;

public record TaskItemDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateOnly CompletedDate { get; set; }
}
