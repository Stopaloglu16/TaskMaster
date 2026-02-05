using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Search
{
    public class FilterOperator : BaseEntity<int>
    {
        public required ColumnType ColumnType { get; set; }

        [Column(TypeName = "varchar(50)")]
        public required string OperatorKey { get; set; } // E.g., "Equals", "Contains", "GreaterThan", etc.

        [Column(TypeName = "varchar(50)")]
        public required string DisplayName { get; set; } // = , != , > , < , Contains, StartsWith, EndsWith, etc.

        [Column(TypeName = "varchar(250)")]
        public required string SqlTemplate { get; set; } // E.g., "{0} = @value", "{0} LIKE '%' + @value + '%'", "{0} > @value", etc.

        public int ValueCount { get; set; } = 0; // Number of values this operator requires (e.g., 1 for Equals, 2 for Between, 0 itself is value)
    }
}
