using Application.Aggregates.TaskItemAggregate.Commands.Create;
using Application.Aggregates.TaskItemAggregate.Commands.Update;
using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Common.Models;


namespace ServiceLayer.TaskItems;

public interface ITaskItemService
{
    Task<TaskItemDto> GetTaskItemId(int Id);
    Task<IEnumerable<SelectListItem>> GetTaskItemList();
    Task<IEnumerable<TaskItemDto>> GetTaskItemsByTaskItem(int taskListId, CancellationToken cancellationToken);
    Task<CustomResult> CreateTaskItem(CreateTaskItemRequest createTaskItemRequest);
    Task<CustomResult> UpdateTaskItem(int Id, UpdateTaskItemRequest updateTaskItemRequest);
    Task<CustomResult> CompleteTaskItem(CompleteTaskItemRequest completeTaskItemRequest);
    Task<CustomResult> SoftDeleteTaskItemById(int Id);
}
