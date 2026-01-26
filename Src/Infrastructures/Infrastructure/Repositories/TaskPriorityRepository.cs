using Application.Aggregates.TaskPriorityAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TaskPriorityRepository : EfCoreRepository<TaskPriority, int>, ITaskPriorityRepository
{

    private readonly ApplicationDbContext _dbContext;

    public TaskPriorityRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<CustomResult<IEnumerable<TaskPrioritySelect>>> GetSelectList()
    {
        var query = _dbContext.Set<TaskPriority>()
                              .AsNoTracking()
                              .OrderBy(tp => tp.Id)
                              .Select(tp => new TaskPrioritySelect
                              {
                                  Id = tp.Id,
                                  Name = tp.Name,
                                  Default = tp.IsDefault
                              });

        var list = await EntityFrameworkQueryableExtensions
                        .ToListAsync(query);

        return CustomResult<IEnumerable<TaskPrioritySelect>>.Success(list);
    }

}
