using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.TaskItemAggregate.Commands.Create;

public record CreateTaskItemRequest
{
    [StringLength(100)]
    public required string Title { get; init; }

    [StringLength(250)]
    public string? Description { get; init; }

    public int TaskListId { get; init; }
}
