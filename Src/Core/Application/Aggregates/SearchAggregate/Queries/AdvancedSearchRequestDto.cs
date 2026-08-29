namespace Application.Aggregates.SearchAggregate.Queries
{

    public record AdvancedSearchRequestDto
    {
        public int AdvancedSearchId { get; set; }

        public List<int> SelectedColumnIds { get; set; } = new();

        public FilterGroupDto? Filters { get; set; }
        public List<SortDefinitionDto> Sorts { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
