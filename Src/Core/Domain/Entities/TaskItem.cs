using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class TaskItem : BaseEntity<int>
{
    [Column(TypeName = "varchar(100)")]
    public required string Title { get; set; }

    [Column(TypeName = "varchar(250)")]
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateOnly CompletedDate { get; set; }
}
