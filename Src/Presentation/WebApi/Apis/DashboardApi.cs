using Application.Aggregates.DashboardAggregate;
using Microsoft.AspNetCore.Http.HttpResults;
using ServiceLayer.Dashboards;

namespace WebApi.Apis
{
    public static class DashboardApi
    {

        public static RouteGroupBuilder DashboardApiV1(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/v{apiVersion:apiVersion}/dashboard")
                                         .HasApiVersion(1.0);
            // Route for query task lists
            api.MapGet("/GetTopWidget", GetTopWidget);
            api.MapGet("/GetMonthlyAnalyse", GetMonthlyAnalyse);
            api.MapGet("/GetTopTaskUsers", GetTopTaskUsers);

            return api;
        }


        public static async Task<Results<Ok<TopWidgetDto>, BadRequest<string>>> GetTopWidget(IDashboardService dashboardService,
                                                             CancellationToken cancellationToken)
        {
            try
            {
                var topWidget = await dashboardService.GetTopWidget(cancellationToken);
                if (topWidget == null)
                {
                    return TypedResults.BadRequest("TopWidget not found.");
                }
                return TypedResults.Ok(topWidget);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest($"An error occurred: {ex.Message}");
            }
        }

        public static async Task<Results<Ok<List<MonthlyAnalyseDto>>, BadRequest<string>>> GetMonthlyAnalyse(IDashboardService dashboardService,
                                                           CancellationToken cancellationToken)
        {
            try
            {
                var monthlyAnalyseList = await dashboardService.GetMonthlyAnalyse(cancellationToken);
                if (monthlyAnalyseList.Count == 0)
                {
                    return TypedResults.BadRequest("TopWidget not found.");
                }
                return TypedResults.Ok(monthlyAnalyseList);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest($"An error occurred: {ex.Message}");
            }
        }

        public static async Task<Results<Ok<List<TopTaskUsersDto>>, BadRequest<string>>> GetTopTaskUsers(IDashboardService dashboardService,
                                                         CancellationToken cancellationToken)
        {
            try
            {
                var topusersList = await dashboardService.GetTopTaskUsersDto(cancellationToken);
                if (topusersList == null)
                {
                    return TypedResults.BadRequest("TopWidget not found.");
                }
                return TypedResults.Ok(topusersList);
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest($"An error occurred: {ex.Message}");
            }
        }
    }
}
