using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories;

public interface ITaskItemRepository : IRepository<TaskItem, int>
{

    Task<CustomResult<int>> CheckMaxTaskItemPerTaskList(int taskListId);

    Task<IEnumerable<TaskItemDto>> GetTaskItems(int taskListId, CancellationToken cancellationToken);
}
