using Domain.Entities;

namespace Application.Aggregates.TaskItemAggregate.Queries
{
    public static class TaskItemMapper
    {
        public static TaskItemDto MapToDto(this TaskItem taskItem)
        {
            return new TaskItemDto
            {
                Title = taskItem.Title,
                Description = taskItem.Description,
                CompletedDate = taskItem.CompletedDate,
                IsCompleted = taskItem.IsCompleted
            };
        }
    }
}
