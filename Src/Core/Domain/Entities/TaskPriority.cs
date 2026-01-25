using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    internal class TaskPriority : BaseEntity<int>
    {
        [Column(TypeName = "varchar(50)")]
        public required string PriorityName { get; set; }

        public required string PriorityIcon { get; set; }
        public required string PriorityColour { get; set; }
    }
}
