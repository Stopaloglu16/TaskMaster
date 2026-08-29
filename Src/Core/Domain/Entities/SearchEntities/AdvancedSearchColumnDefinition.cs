using Domain.Common;

namespace Domain.Entities.SearchEntities;

public class AdvancedSearchColumnDefinition : BaseEntity<int>
{
    public int TableId { get; set; }
    public AdvancedSearchTable Table { get; set; } = null!;

    public required string ColumnName { get; set; }     // Real DB column
    public required string DisplayName { get; set; }

    public int ColumnTypeId { get; set; }
    public ColumnType ColumnType { get; set; } = null!;
}
