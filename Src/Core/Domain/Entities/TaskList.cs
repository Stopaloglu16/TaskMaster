using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class TaskList : BaseAuditableEntity<int>
{
    [Column(TypeName = "varchar(100)")]
    public required string Title { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateOnly DueDate { get; set; }
    public DateOnly? CompletedDate { get; set; }

    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public IList<TaskItem> TaskItems { get; private set; } = new List<TaskItem>();
}