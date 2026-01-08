using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
using Domain.Entities;

namespace Application.Aggregates.TaskItemAggregate.Queries
{
    public static class TaskItemMapper
    {
        public static TaskItemDto MapToDto(this TaskItem taskItem)
        {
            return new TaskItemDto
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                CompletedDate = taskItem.CompletedDate,
                IsCompleted = taskItem.IsCompleted
            };
        }

        public static TaskItemFormRequest MapToFormDto(this TaskItem taskItem)
        {
            return new TaskItemFormRequest()
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                TaskListId = taskItem.TaskListId
            };
        }

    }
}
