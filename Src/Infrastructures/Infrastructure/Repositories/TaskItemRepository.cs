using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
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

    public async Task<TaskItemFormRequest?> GetTaskItemById(int Id, CancellationToken cancellationToken)
    {
        var tempTaskItem = await _dbContext.TaskItems.AsNoTracking()
                                      .Where(qq => qq.IsDeleted == 0 && qq.Id == Id)
                                      .FirstOrDefaultAsync(cancellationToken);

        return tempTaskItem?.MapToFormDto();
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

    public async Task<CustomResult<int>> CheckMaxTaskItemPerTaskList(int taskListId)
    {

        var taskItemCount = await _dbContext.TaskItems.CountAsync(q => q.TaskListId == taskListId && q.IsCompleted == false);

        return CustomResult<int>.Success(taskItemCount);
    }

    public async Task<CustomResult> CompleteSingleTaskItem(int taskItemId, CancellationToken cancellationToken)
    {
        FormattableString queryString = $"""
            UPDATE [dbo].[TaskItems] 
            SET IsCompleted = 1, [CompletedDate] = GETDATE()
            WHERE Id = {taskItemId}
            """;

        await _dbContext.Database.ExecuteSqlAsync(queryString, cancellationToken);

        return CustomResult.Success();
    }
}
