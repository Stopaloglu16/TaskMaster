using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Search
{
    public class SearchColumnFilter : BaseEntity<int>
    {
        [Column(TypeName = "varchar(50)")]
        public required string ColumnName { get; set; }

        [Column(TypeName = "varchar(50)")]
        public required string DisplayName { get; set; }

        public ColumnType ColumnType { get; set; } // E.g., String, Number, Date, etc.
        public bool IsSearchable { get; set; } = false;
        public int OperatorId { get; set; }

        public int SearchTypeId { get; set; }
        public SearchType SearchType { get; set; } = default!;

    }
}
