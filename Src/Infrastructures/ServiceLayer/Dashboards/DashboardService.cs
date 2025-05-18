using Application.Aggregates.DashboardAggregate;
using Application.Repositories;

namespace ServiceLayer.Dashboards;

public class DashboardService : IDashboardService
{

    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<TopWidgetDto> GetTopWidget(CancellationToken cancellationToken)
    {
        return await _dashboardRepository.GetTopWidget(cancellationToken);
    }

    public async Task<List<MonthlyAnalyseDto>> GetMonthlyAnalyse(CancellationToken cancellationToken)
    {
        return await _dashboardRepository.GetMonthlyAnalyse(cancellationToken);
    }

    public async Task<List<TopTaskUsersDto>> GetTopTaskUsersDto(CancellationToken cancellationToken)
    {
        return await _dashboardRepository.GetTopTaskUsersDto(cancellationToken);
    }

 
}
