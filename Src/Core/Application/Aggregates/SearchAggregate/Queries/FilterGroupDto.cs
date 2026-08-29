using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Aggregates.SearchAggregate.Queries
{
    public record FilterGroupDto
    {
        public LogicalOperator Logic { get; set; }   // AND / OR

        public List<FilterItemDto> Items { get; set; } = new();
    }
}
