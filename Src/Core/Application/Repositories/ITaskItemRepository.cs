using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories;

public interface ITaskItemRepository : IRepository<TaskItem, int>
{

    Task<TaskItemFormRequest?> GetTaskItemById(int Id, CancellationToken cancellationToken);
    Task<CustomResult<int>> CheckMaxTaskItemPerTaskList(int taskListId);

    Task<IEnumerable<TaskItemDto>> GetTaskItems(int taskListId, CancellationToken cancellationToken);
}
