using Application.Aggregates.SearchAggregate.Queries;
using Application.Common.Models;

namespace ServiceLayer.AdvancedSearches
{
    public interface IAdvancedSearchService
    {
        Task<CustomResult<AdvancedSearchResponseDto>> Search(AdvancedSearchRequestDto request,
                                                             CancellationToken cancellationToken);
    }
}
