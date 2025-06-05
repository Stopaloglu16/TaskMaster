using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate
{
    public record CreateTaskItemRequest
    {
        public int RowId { get; set; }
        [StringLength(100)]
        public required string Title { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }
    }
}
