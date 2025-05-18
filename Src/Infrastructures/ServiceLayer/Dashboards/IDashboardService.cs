using Application.Aggregates.DashboardAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Dashboards;

public interface IDashboardService
{
    Task<TopWidgetDto> GetTopWidget(CancellationToken cancellationToken);

    Task<List<MonthlyAnalyseDto>> GetMonthlyAnalyse(CancellationToken cancellationToken);
    Task<List<TopTaskUsersDto>> GetTopTaskUsersDto(CancellationToken cancellationToken);

}
