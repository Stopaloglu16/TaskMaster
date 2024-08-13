using Application.Aggregates.TaskItemAggregate.Commands.Create;
using Application.Aggregates.TaskItemAggregate.Commands.Update;
using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Common.Models;
using System.Web.Mvc;

namespace ServiceLayer.TaskItems;

public interface ITaskItemService
{
    Task<TaskItemDto> GetTaskItemId(int Id);
    Task<IEnumerable<SelectListItem>> GetTaskItemList();
    Task<IEnumerable<TaskItemDto>> GetTaskItems(int taskListId);
    Task<CustomResult> CreateTaskItem(CreateTaskItemRequest createTaskItemRequest);
    Task<CustomResult> UpdateTaskItem(UpdateTaskItemRequest updateTaskItemRequest);
    Task<CustomResult> CompleteTaskItem(CompleteTaskItemRequest completeTaskItemRequest);
    Task<CustomResult> SoftDeleteTaskListById(int Id);
}
