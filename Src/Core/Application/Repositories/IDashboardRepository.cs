using Application.Aggregates.DashboardAggregate;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Repositories
{

    public interface IDashboardRepository : IRepository<TaskList, int>
    {
        Task<TopWidgetDto> GetTopWidget(CancellationToken cancellationToken);

        Task<List<MonthlyAnalyseDto>> GetMonthlyAnalyse(CancellationToken cancellationToken);
        Task<List<TopTaskUsersDto>> GetTopTaskUsersDto(CancellationToken cancellationToken);
    }
}
