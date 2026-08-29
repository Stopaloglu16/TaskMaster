using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.SearchEntities;

public class AdvancedSearchJoin : BaseEntity<int>
{
    public int AdvancedSearchId { get; set; }
    public AdvancedSearch AdvancedSearch { get; set; } = null!;

    public int FromTableAliasId { get; set; }
    public int ToTableAliasId { get; set; }

    [Column(TypeName = "varchar(10)")]
    public string JoinType { get; set; } = "LEFT"; // LEFT, INNER

    [Column(TypeName = "varchar(100)")]
    public string JoinCondition { get; set; } = null!; // ti.TaskListId = tl.Id
}
