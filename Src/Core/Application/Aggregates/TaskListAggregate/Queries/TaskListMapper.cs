using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Domain.Entities;

namespace Application.Aggregates.TaskListAggregate.Queries
{
    public static class TaskListMapper
    {
        public static TaskListDto MapToDto(this TaskList taskList)
        {
            return new TaskListDto
            {
                Id = taskList.Id,
                Title = taskList.Title,
                DueDate = taskList.DueDate,
                AssignedTo = taskList.AssignedTo?.FullName ?? "",
                TaskItemCount = taskList.TaskItems.Count(),
                TaskItemCompletedCount = taskList.TaskItems.Count(ti => ti.IsCompleted),
            };
        }

        public static TaskListFormRequest? MapToFormDto(this TaskList taskList)
        {
            return new TaskListFormRequest
            {
                Id = taskList.Id,
                Title = taskList.Title,
                DueDate = taskList.DueDate,
                AssignedToId = taskList.AssignedToId
            };
        }
    }
}
