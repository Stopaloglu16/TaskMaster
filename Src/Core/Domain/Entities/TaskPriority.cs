using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class TaskPriority : BaseEntity<int>
    {
        [Column(TypeName = "varchar(50)")]
        public required string Name { get; set; }        // Low, Medium, High
        [Column(TypeName = "varchar(50)")]
        public required string Color { get; set; }       // "secondary", "danger", "#dc3545"
        public int SortOrder { get; set; }                // 1,2,3...
        public bool IsDefault { get; set; } = false;
        public ICollection<TaskList> Tasks { get; set; } = new List<TaskList>();
    }
}
