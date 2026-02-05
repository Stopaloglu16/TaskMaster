namespace Application.Aggregates.SearchAggregate.Queries
{

    public record SearchRequestDto
    {
        public int SearchTypeId { get; init; }
        public List<int> SelectedColumnFieldIds { get; set; } = new();
        public List<FilterRuleDto> Filters { get; set; } = new();

        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;

        public bool IsExport { get; init; } = false;
    }
}
