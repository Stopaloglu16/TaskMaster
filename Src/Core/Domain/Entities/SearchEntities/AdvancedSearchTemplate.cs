using Domain.Common;

namespace Domain.Entities.SearchEntities;

public class AdvancedSearchTemplate : BaseAuditableEntity<int>
{
    public int AdvancedSearchId { get; set; }
    public required string Name { get; set; }

    public string JsonDefinition { get; set; }  // serialized filter structure
}
