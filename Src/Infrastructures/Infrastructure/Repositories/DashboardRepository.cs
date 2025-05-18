using Application.Aggregates.DashboardAggregate;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DashboardRepository : EfCoreRepository<TaskList, int>, IDashboardRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<TopWidgetDto> GetTopWidget(CancellationToken cancellationToken)
    {
        TopWidgetDto topWidget = new TopWidgetDto();

        topWidget.taskListPercentages = await _dbContext.TaskLists
            .Where(x => x.DueDate >= DateOnly.FromDateTime(DateTime.Now) && x.DueDate <= DateOnly.FromDateTime(DateTime.Now.AddDays(5)))
            .GroupBy(x => x.DueDate)
            .Select(x => new TaskListPercentage
            {
                DueDate = x.Key.ToString(),
                TotalTasks = x.Count(),
                OpenTasks = x.Count(t => t.IsCompleted == false)
            })
            .ToListAsync(cancellationToken);

        topWidget.taskItemPercentages = await _dbContext.TaskItems
            .Where(x => x.TaskList.DueDate >= DateOnly.FromDateTime(DateTime.Now) && x.TaskList.DueDate <= DateOnly.FromDateTime(DateTime.Now.AddDays(5)))
            .GroupBy(x => x.TaskList.DueDate)
            .Select(x => new TaskItemPercentage
            {
                DueDateTaskItem = x.Key.ToString(),
                OpenTaskItems = x.Count(t => t.IsCompleted == false)
            })
            .ToListAsync(cancellationToken);


        return topWidget;
    }


    public async Task<List<MonthlyAnalyseDto>> GetMonthlyAnalyse(CancellationToken cancellationToken)
    {

        var monthlyAnalyseDtoList = await _dbContext.TaskLists
            .Where(x => x.DueDate >= DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)) && x.DueDate <= DateOnly.FromDateTime(DateTime.Now))
            .GroupBy(x => x.DueDate)
            .Select(x => new
            {
                DueDate = x.Key,
                CompletedInTime = x.Count(t => t.IsCompleted == true && t.DueDate >= t.CompletedDate),
                NotCompleted = x.Count(t => t.IsCompleted == false),
                CompletedLate = x.Count(t => t.IsCompleted == true && t.CompletedDate > t.DueDate)
            })
            .ToListAsync(cancellationToken);

        var result = monthlyAnalyseDtoList
              .GroupBy(x => x.DueDate.ToString("MMM"))
              .Select(g => new MonthlyAnalyseDto
              {
                  MonthName = g.Key,
                  CompletedInTime = g.Sum(x => x.CompletedInTime),
                  NotCompleted = g.Sum(x => x.NotCompleted),
                  CompletedLate = g.Sum(x => x.CompletedLate)
              })
              .OrderBy(x => DateTime.ParseExact(x.MonthName, "MMM", null).Month)
              .ToList();

        return result;
    }

    public async Task<List<TopTaskUsersDto>> GetTopTaskUsersDto(CancellationToken cancellationToken)
    {
        return  await _dbContext.TaskLists
            .Where(x => x.DueDate >= DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)) && x.DueDate <= DateOnly.FromDateTime(DateTime.Now))
            .GroupBy(x => x.AssignedTo.FullName)
            .Select(x => new TopTaskUsersDto
            {
                Username  = x.Key,
                TaskCount = x.Count()
            })
            .Take(5)
            .OrderBy(x => x.TaskCount)
            .ToListAsync(cancellationToken);
    }

}
