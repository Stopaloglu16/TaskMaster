using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Aggregates.FileJobAggregate.Commands
{
    public class CreateFileJobUploadRequest
    {
        /// <summary>
        /// Client-generated, stable per CSV row. The upload page posts one chunk per 10 rows, so an
        /// N-row file is several independent requests; a unique index on (FileJobId, BatchKey) makes
        /// a retried chunk a no-op instead of a duplicate.
        /// </summary>
        [Required]
        [StringLength(64)]
        public required string BatchKey { get; set; }

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
