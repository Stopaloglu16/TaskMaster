using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.TaskListAggregate.Commands.Update;

public record UpdateTaskListRequest
{
    [StringLength(100)]
    public required string Title { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly CompletedDate { get; set; }
    public int? AssignedToId { get; set; }
}
