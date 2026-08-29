using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Aggregates.SearchAggregate.Queries
{
    public record AdvancedSearchResponseDto
    {
        public List<AdvancedSearchColumnDto> Columns { get; set; } = new();
        public List<object[]> Rows { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public record AdvancedSearchColumnDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
