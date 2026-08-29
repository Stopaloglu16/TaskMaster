using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.SearchEntities;

public class AdvancedSearch : BaseEntity<int>
{
    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; } = null!;

    public int MainTableId { get; set; }
    public AdvancedSearchTable MainTable { get; set; } = null!;


    public ICollection<AdvancedSearchColumn> AdvancedSearchColumns { get; set; } = new List<AdvancedSearchColumn>();

    public ICollection<AdvancedSearchTemplate> Templates { get; set; } = new List<AdvancedSearchTemplate>();
}
