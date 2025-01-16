using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
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
        var taskListCount = await _dbContext.TaskLists.AsNoTracking()
                                                          .CountAsync(q => q.AssignedToId == userId && q.IsCompleted == false);

        return CustomResult<int>.Success(taskListCount);
    }

    public async Task<TaskListFormRequest?> GetTaskListById(int Id, CancellationToken cancellationToken)
    {
        return await _dbContext.TaskLists.AsNoTracking()
                                         .Where(qq => qq.IsDeleted == 0 &&
                                                             qq.IsCompleted == false)
                                         .Select(ss => new TaskListFormRequest
                                         {
                                             Id = ss.Id,
                                             Title = ss.Title,
                                             DueDate = ss.DueDate,
                                             AssignedToId = ss.AssignedToId
                                         })
                                         .FirstOrDefaultAsync(qq => qq.Id == Id, cancellationToken);
    }


    public async Task<PagingResponse<TaskListDto>> GetActiveTaskListWithPagination(PagingParameters pagingParameters,
                                                                                   CancellationToken cancellationToken)
    {

        var query = _dbContext.TaskLists.Include(ss => ss.AssignedTo)
                                         .AsNoTracking()
                                         .Where(qq => qq.IsDeleted == 0 &&
                                                             qq.IsCompleted == false)
                                         .Select(ss => new TaskListDto
                                         {
                                             Id = ss.Id,
                                             Title = ss.Title,
                                             DueDate = ss.DueDate,
                                             AssignedTo = ss.AssignedTo.FullName ?? "",
                                             TaskItemCount = ss.TaskItems.Count(),
                                             TaskItemCompletedCount = ss.TaskItems.Count(ti => ti.IsCompleted),
                                         });

        return await PagingResponse<TaskListDto>.CreateAsync(query, pagingParameters);

    }
}
