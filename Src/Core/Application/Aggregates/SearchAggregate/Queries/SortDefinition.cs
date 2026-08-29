using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Aggregates.SearchAggregate.Queries
{
    public record SortDefinitionDto
    {
        public int ReportColumnId { get; set; }
        public bool Descending { get; set; }
    }
}
