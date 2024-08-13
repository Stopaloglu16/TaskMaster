using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TaskListRepository : EfCoreRepository<TaskList, int>, ITaskListRepository
{

    private readonly ApplicationDbContext _dbContext;

    public TaskListRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomResult<int>> CheckMaxTaskListPerUser(int userId)
    {
        var taskListCount = await _dbContext.TaskLists.CountAsync(q => q.AssignedToId == userId && q.IsCompleted == false);

        return CustomResult<int>.Success(taskListCount);
    }

    public async Task<IEnumerable<TaskListDto>> GetTaskListList()
    {
        return await _dbContext.TaskLists.Include(ss => ss.AssignedTo)
                                         .Where(qq => qq.IsDeleted == 0)
                                         .Select(ss => new TaskListDto
                                         {
                                             Id = ss.Id,
                                             Title = ss.Title,
                                             DueDate = ss.DueDate,
                                             AssignedTo = ss.AssignedTo.FullName
                                         }).ToListAsync();
    }
}
