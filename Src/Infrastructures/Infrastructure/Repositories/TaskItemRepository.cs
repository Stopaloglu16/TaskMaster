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
        try
        {
            return await _dbContext.TaskItems.AsNoTracking()
                                       .Where(ti => ti.TaskListId == taskListId && ti.IsDeleted == 0)
                                       .Select(ti => ti.MapToDto())
                                       .ToListAsync();
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}
