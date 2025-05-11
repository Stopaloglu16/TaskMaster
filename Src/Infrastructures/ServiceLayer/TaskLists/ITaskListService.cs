using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;

namespace ServiceLayer.TaskLists;

public interface ITaskListService
{
    Task<TaskListDto> GetTaskListId(int Id);

    Task<CustomResult> CreateTaskList(TaskListFormRequest taskListFormRequest);
    Task<CustomResult> UpdateTaskList(int Id, TaskListFormRequest taskListFormRequest);
    Task<CustomResult> SoftDeleteTaskListById(int Id);

    Task<CustomResult<TaskListDto>> GetTaskListById(int Id, CancellationToken cancellationToken);
    Task<CustomResult<TaskListFormRequest>> GetTaskListFormById(int Id, CancellationToken cancellationToken);
    
    Task<IEnumerable<SelectListItem>> GetTaskListSelectList();
    Task<PagingResponse<TaskListDto>> GetActiveTaskListWithPagination(PagingParameters pagingParameters, CancellationToken cancellationToken);
    Task<IEnumerable<TaskListWithItemsDto>> GetTaskListWithItemsByUser(int userId, CancellationToken cancellationToken);
    Task<CustomResult> CompleteTaskList(int Id, CancellationToken cancellationToken);
}
