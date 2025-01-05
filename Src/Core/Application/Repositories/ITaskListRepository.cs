using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories;

public interface ITaskListRepository : IRepository<TaskList, int>
{
    Task<PagingResponse<TaskListDto>> GetActiveTaskListWithPagination(PagingParameters pagingParameters, CancellationToken cancellationToken);

    Task<TaskListFormRequest?> GetTaskListById(int Id, CancellationToken cancellationToken);
    Task<CustomResult<int>> CheckMaxTaskListPerUser(int userId);
}
