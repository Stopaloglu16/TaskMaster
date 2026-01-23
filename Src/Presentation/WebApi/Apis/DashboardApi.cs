using Application.Aggregates.DashboardAggregate;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Dashboards;
using Serilog;

namespace WebApi.Apis
{
    public static class DashboardApi
    {
   
        public static RouteGroupBuilder DashboardApiV1(this RouteGroupBuilder group)
        {
            
            // Route for query task lists
            group.MapGet("/GetTopWidget", GetTopWidget);
            group.MapGet("/GetMonthlyAnalyse", GetMonthlyAnalyse);
            group.MapGet("/GetTopTaskUsers", GetTopTaskUsers);


            return group;
        }


        public static async Task<Results<Ok<TopWidgetDto>, BadRequest<string>>> GetTopWidget(IDashboardService dashboardService,
            //ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                //logger.LogInformation("Fetching top widget data from the dashboard service.");
                var topWidget = await dashboardService.GetTopWidget(cancellationToken);
                if (topWidget == null)
                {
                    return TypedResults.BadRequest("TopWidget not found.");
                }

                //throw new Exception("This is a test exception to check the error handling in the API.");
                return TypedResults.Ok(topWidget);
            }
            catch (Exception ex)
            {
                Log.Warning($"Fetching top widget data {ex.Message}");
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
