using Domain.Common;

namespace Domain.Entities.SearchEntities
{
    public class AdvancedSearchTable : BaseEntity<int>
    {
        public required string TableName { get; set; }     // Real DB table name
        public required string TableAlias { get; set; }    // Default alias

        // Parent relation (for upward join chain)
        public int? ParentTableId { get; set; }
        public AdvancedSearchTable? ParentTable { get; set; }

        // FK from this table to parent
        public string? ForeignKeyColumn { get; set; }   // e.g. T1Id
        public string? ParentKeyColumn { get; set; }    // e.g. Id

        public ICollection<AdvancedSearchColumnDefinition> Columns { get; set; }
    }
}
