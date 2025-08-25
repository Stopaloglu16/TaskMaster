using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class TaskListFileUpload : BaseEntity<int>
    {
        [Column(TypeName = "varchar(150)")]
        public required string TaskTitle { get; set; }
        public DateOnly DueDate { get; set; }
        public int? AssignedToId { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? AssignedTo { get; set; }


        [Column(TypeName = "varchar(150)")]
        public required string Title { get; set; }

        [Column(TypeName = "varchar(350)")]
        public string? Description { get; set; }


        public RowStatus Status { get; set; } = RowStatus.New;
        [Column(TypeName = "varchar(350)")]
        public string? ErrorMessage { get; set; }
        

        public int TaskListFileJobId { get; set; }
        public TaskListFileJob TaskListFileJob { get; set; }
    }
}
