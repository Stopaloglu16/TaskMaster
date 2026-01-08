using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskItemAggregate.Commands.Update;
using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Common.Models;


namespace ServiceLayer.TaskItems;

public interface ITaskItemService
{
    Task<CustomResult<TaskItemFormRequest>> GetTaskItemId(int Id, CancellationToken cancellationToken);
    Task<IEnumerable<SelectListItem>> GetTaskItemList();
    Task<IEnumerable<TaskItemDto>> GetTaskItemsByTaskItem(int taskListId, CancellationToken cancellationToken);
    
    Task<CustomResult> CreateTaskItem(TaskItemFormRequest taskItemFormRequest);
    Task<CustomResult> UpdateTaskItem(int Id, TaskItemFormRequest taskItemFormRequest);
    Task<CustomResult> CompleteSingleTaskItem(CompleteTaskItemRequest completeTaskItemRequest, CancellationToken cancellationToken);
    Task<CustomResult> SoftDeleteTaskItemById(int Id);
}
