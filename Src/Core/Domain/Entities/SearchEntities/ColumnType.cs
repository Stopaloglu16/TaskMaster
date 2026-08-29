using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.SearchEntities;

public class ColumnType : BaseEntity<int>
{
    [Column(TypeName = "varchar(100)")]
    public string Code { get; set; } = null!;   // text, number, date, list

    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; } = null!;

    public ICollection<Operator> Operators { get; set; } = new List<Operator>();
}
