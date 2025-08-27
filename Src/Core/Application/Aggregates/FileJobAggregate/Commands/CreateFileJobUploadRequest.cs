using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Aggregates.FileJobAggregate.Commands
{
    public class CreateFileJobUploadRequest
    {
        [StringLength(150)]
        public required string TaskTitle { get; set; }
        
        public DateOnly DueDate { get; set; }
        
        [StringLength(60)]
        public string? AssignedTo { get; set; }
        
        [StringLength(150)]
        public required string Title { get; set; }

        [Column(TypeName = "varchar(350)")]
        public string? Description { get; set; }

    }
}
