using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;

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

    public async Task<TaskListFormRequest?> GetTaskListFormById(int Id, CancellationToken cancellationToken)
    {
        var tempTaskList = await _dbContext.TaskLists.AsNoTracking()
                                         .Where(qq => qq.IsDeleted == 0)
                                         .FirstOrDefaultAsync(qq => qq.Id == Id, cancellationToken);

        return tempTaskList?.MapToFormDto();
    }

    public async Task<TaskListDto?> GetTaskListById(int Id, CancellationToken cancellationToken)
    {
        var tempTaskList = await _dbContext.TaskLists.AsNoTracking()
                                         .Where(qq => qq.IsDeleted == 0)
                                         .FirstOrDefaultAsync(qq => qq.Id == Id, cancellationToken);

        return tempTaskList?.MapToDto();
    }

    public async Task<PagingResponse<TaskListDto>> GetActiveTaskListWithPagination(PagingParameters pagingParameters,
                                                                                   CancellationToken cancellationToken)
    {
        var query = _dbContext.TaskLists.Include(ss => ss.AssignedTo)
                                         .Include(ss => ss.TaskItems)
                                         .AsNoTracking()
                                         .Where(qq => qq.IsCompleted == false)
                                         .Select(ss => ss.MapToDto());

        return await PagingResponse<TaskListDto>.CreateAsync(query, pagingParameters);

    }

    public async Task<IEnumerable<TaskListWithItemsDto>> GetTaskListWithItemsByUser(int userId, CancellationToken cancellationToken)
    {
        try
        {
            //var mylist1 = await _dbContext.TaskLists.ToListAsync();

            var mylist =  await _dbContext.TaskLists
                   .Where(t => t.TaskItems.Any() && t.AssignedToId == userId && t.IsCompleted == false)
                   .Select(t => new TaskListWithItemsDto
                   {
                       Id = t.Id,
                       Title = t.Title,
                       DueDate = t.DueDate,
                       CompletedDate = t.CompletedDate,
                       taskItemDtos = t.TaskItems.Select(i => new TaskItemDto
                       {
                           Id = i.Id,
                           Title = i.Title,
                           Description = i.Description,
                           IsCompleted = i.IsCompleted
                       }).ToList()
                   })
                   .ToListAsync();

            return mylist;

        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<CustomResult> CompleteTaskList(int Id, CancellationToken cancellationToken)
    {

        FormattableString queryString = $"""
        UPDATE [dbo].[TaskLists]
        SET [IsCompleted] = 1, [CompletedDate] = GETDATE()
        WHERE Id = {Id} AND NOT EXISTS (
            SELECT 1
            FROM [dbo].[TaskItems]
            WHERE TaskListId = {Id} AND IsCompleted = 0 AND IsDeleted = 0
        )
    """;


        await _dbContext.Database.ExecuteSqlAsync(queryString, cancellationToken);

        return CustomResult.Success();

    }
}
