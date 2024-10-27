using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TaskItemRepository : EfCoreRepository<TaskItem, int>, ITaskItemRepository
{

    private readonly ApplicationDbContext _dbContext;

    public TaskItemRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomResult<int>> CheckMaxTaskItemPerTaskList(int taskListId)
    {

        var taskItemCount = await _dbContext.TaskItems.CountAsync(q => q.TaskListId == taskListId && q.IsCompleted == false);

        return CustomResult<int>.Success(taskItemCount);
    }

    public async Task<IEnumerable<TaskItemDto>> GetTaskItems(int taskListId, CancellationToken cancellationToken)
    {
        return await _dbContext.TaskItems.AsNoTracking()
                                         .Where(ti => ti.TaskListId == taskListId)
                                         .Select(ti => new TaskItemDto
                                         {
                                             Title = ti.Title,
                                             Description = ti.Description,
                                             CompletedDate = ti.CompletedDate,
                                             IsCompleted = ti.IsCompleted
                                         })
                                         .ToListAsync();
    }
}
