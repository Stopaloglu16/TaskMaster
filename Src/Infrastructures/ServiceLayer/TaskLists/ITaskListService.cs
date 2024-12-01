using Application.Aggregates.TaskListAggregate.Commands.Create;
using Application.Aggregates.TaskListAggregate.Commands.Update;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;

namespace ServiceLayer.TaskLists;

public interface ITaskListService
{
    Task<TaskListDto> GetTaskListId(int Id);
    Task<IEnumerable<SelectListItem>> GetTaskListSelectList();
    Task<PagingResponse<TaskListDto>> GetActiveTaskListWithPagination(PagingParameters pagingParameters, CancellationToken cancellationToken);
    Task<CustomResult> CreateTaskList(CreateTaskListRequest createTaskListRequest);
    Task<CustomResult> UpdateTaskList(int Id, UpdateTaskListRequest updateTaskListRequest);
    Task<CustomResult> SoftDeleteTaskListById(int Id);
}
