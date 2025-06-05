using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Aggregates.TaskListAggregate.Commands.CreateUpdate
{
    public record CreateTaskListRequest
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Max 100 chars")]
        public required string Title { get; set; }
        public DateOnly DueDate { get; set; }
        
        [JsonIgnore]
        public int? AssignedToId { get; set; }

        public string? AssignedTo { get; set; }

        public List<CreateTaskItemRequest> createTaskItemRequests { get; set; } = new List<CreateTaskItemRequest>();
    }
}
