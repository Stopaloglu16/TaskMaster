using Application.Aggregates.TaskItemAggregate.Queries;

namespace Application.Aggregates.TaskListAggregate.Queries;

public record TaskListWithItemsDto
{
    public int Id { get; init; }
    public string Title { get; init; }
    public DateOnly DueDate { get; init; }
    public DateOnly? CompletedDate { get; init; }
    public string? AssignedTo { get; set; }

    public List<TaskItemDto> taskItemDtos { get; set; } = new List<TaskItemDto>();
}
