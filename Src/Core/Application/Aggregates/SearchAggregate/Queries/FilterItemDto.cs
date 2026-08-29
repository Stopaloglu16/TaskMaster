using Domain.Enums;

namespace Application.Aggregates.SearchAggregate.Queries
{
    public record FilterItemDto
    {
        // Single filter
        public int? ColumnId { get; set; }
        public string? OperatorCode { get; set; }
        public object? Value { get; set; }
        public object? ValueTo { get; set; } // for range

        // Group (1-level only)
        public LogicalOperator? GroupLogic { get; set; }
        public List<FilterItemDto>? Group { get; set; }
    }
}
