using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories;

public interface ITaskListRepository : IRepository<TaskList, int>
{
    Task<PagingResponse<TaskListDto>> GetActiveTaskListWithPagination(PagingParameters pagingParameters, CancellationToken cancellationToken);
    Task<IEnumerable<TaskListWithItemsDto>> GetTaskListWithItemsByUser(int userId, CancellationToken cancellationToken);
    Task<TaskListFormRequest?> GetTaskListFormById(int Id, CancellationToken cancellationToken);
    Task<TaskListDto?> GetTaskListById(int Id, CancellationToken cancellationToken);
    Task<CustomResult<int>> CheckMaxTaskListPerUser(int userId);
    Task<CustomResult> CompleteTaskList(int Id, CancellationToken cancellationToken);
}
