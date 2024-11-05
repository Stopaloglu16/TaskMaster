using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.TaskListAggregate.Commands.Create;

public record CreateTaskListRequest
{
    [StringLength(100)]
    public required string Title { get; set; }
    public DateOnly DueDate { get; set; }
    public int? AssignedToId { get; set; } = null;
}
