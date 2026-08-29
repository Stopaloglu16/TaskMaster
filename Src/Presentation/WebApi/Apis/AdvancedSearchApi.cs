using Application.Aggregates.SearchAggregate.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.AdvancedSearches;

namespace WebApi.Apis
{
    public static class AdvancedSearchApi
    {
        public static RouteGroupBuilder AdvancedSearchApiV1(this RouteGroupBuilder group)
        {
            // Route for query task lists
            group.MapPost("/", GetResultsWithPagination)
                 .WithSummary("Get active task lists")
                 .WithDescription("Returns a paginated list of active task lists.");

            return group;
        }

        
        public static async Task<Ok<AdvancedSearchResponseDto>> GetResultsWithPagination([FromBody] AdvancedSearchRequestDto advancedSearchRequestDto, 
                                                                                         [FromServices]  IAdvancedSearchService advancedSearchService,
                                                                                         CancellationToken cancellationToken)
        {

            var taskList = await advancedSearchService.Search(advancedSearchRequestDto, cancellationToken);
            return TypedResults.Ok(taskList.Value);
        }
    }
}
