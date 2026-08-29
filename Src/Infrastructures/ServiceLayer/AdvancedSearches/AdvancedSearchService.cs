using Application.Aggregates.SearchAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Microsoft.Extensions.Logging;

namespace ServiceLayer.AdvancedSearches
{
    public class AdvancedSearchService : IAdvancedSearchService
    {

        private readonly IAdvancedSearchRepository _advancedSearchRepository;

        private readonly ILogger<AdvancedSearchService> _logger;

        public AdvancedSearchService(IAdvancedSearchRepository advancedSearchRepository,
                                     ILogger<AdvancedSearchService> logger)
        {
            _advancedSearchRepository = advancedSearchRepository;
            _logger = logger;
        }

        public async Task<CustomResult<AdvancedSearchResponseDto>> Search(AdvancedSearchRequestDto request,
                                                                          CancellationToken cancellationToken)
        {
            try
            {
                var results = await _advancedSearchRepository.RunAdvancedSearchAsync(request, cancellationToken);
                return CustomResult<AdvancedSearchResponseDto>.Success(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during advanced search.");
                return CustomResult<AdvancedSearchResponseDto>.Failure(
                        new CustomError(false, "An error occurred while performing the search."));
            }

        }
    }
}
