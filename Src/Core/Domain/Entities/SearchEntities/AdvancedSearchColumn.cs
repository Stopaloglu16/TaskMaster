using Domain.Common;

namespace Domain.Entities.SearchEntities;

public class AdvancedSearchColumn : BaseEntity<int>
{
    public int AdvancedSearchId { get; set; }
    public AdvancedSearch AdvancedSearch { get; set; } = null!;

    public int ColumnDefinitionId { get; set; }
    public AdvancedSearchColumnDefinition ColumnDefinition { get; set; } = null!;

    // Behavior
    public bool IsSelectable { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsSortable { get; set; }

}
