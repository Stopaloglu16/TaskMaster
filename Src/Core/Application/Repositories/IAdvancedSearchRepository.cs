using Application.Aggregates.SearchAggregate.Queries;
using Application.Common.Interfaces;
using Domain.Entities.SearchEntities;

namespace Application.Repositories
{
    public interface IAdvancedSearchRepository : IRepository<AdvancedSearch, int>
    {

        Task<AdvancedSearchResponseDto> RunAdvancedSearchAsync(AdvancedSearchRequestDto request, CancellationToken cancellationToken);
    }
}
