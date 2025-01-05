using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;

public record TaskListFormRequest
{
    public int Id { get; set; }

    [StringLength(100)]
    public required string Title { get; set; }
    public DateOnly DueDate { get; set; }
    public int? AssignedToId { get; set; }
}
