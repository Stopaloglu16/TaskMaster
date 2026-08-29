using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.SearchEntities;

public class Operator : BaseEntity<int>
{
    [Column(TypeName = "varchar(100)")]
    public string Code { get; set; } = null!;          // equal, today, between

    [Column(TypeName = "varchar(100)")]
    public string DisplayName { get; set; } = null!;

    // SQL
    [Column(TypeName = "varchar(150)")]
    public string SqlTemplate { get; set; } = null!;  // {col} BETWEEN @p1 AND @p2

    // Value behavior
    public OperatorValueMode ValueMode { get; set; }  // None, Single, Range

    // UI behavior
    public InputControlType InputControl { get; set; } // None, TextBox, DatePicker


    public List<ColumnType> ColumnTypes { get; set; } = new List<ColumnType>();
}
