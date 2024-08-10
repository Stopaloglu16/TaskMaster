namespace Application.Aggregates.TaskListAggregate.Queries;

public record TaskListDto
{
    public int Id { get; init; }
    public string Title { get; init; }
    public bool IsCompleted { get; init; }
    public DateOnly DueDate { get; init; }
    public DateOnly CompletedDate { get; init; }
    public string? AssignedTo { get; set; }


}
