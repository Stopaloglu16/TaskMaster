using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.TaskItemAggregate.Commands.Update;

public record UpdateTaskItemRequest
{
    public int Id { get; set; }

    [StringLength(100)]
    public required string Title { get; init; }

    [StringLength(250)]
    public string? Description { get; init; }

}
