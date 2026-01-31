using Application.Aggregates.TaskPriorityAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
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
    
    public async Task<IEnumerable<SelectListItem>> GetSelectList()
    {
        var tp = await _dbContext.TaskPriority
                              .AsNoTracking()
                              .OrderBy(tp => tp.SortOrder)
                              .Select(ss => new SelectListItem()
                              {
                                  Value = ss.Id,
                                  Text = ss.Name
                              }).ToListAsync();

        return tp;

    }

}
