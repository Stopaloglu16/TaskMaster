using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class FileJobUpload : BaseEntity<int>
    {
        /// <summary>
        /// Client-generated row key, unique within a FileJob. Makes re-posting an upload chunk safe.
        /// </summary>
        [Column(TypeName = "varchar(64)")]
        public required string BatchKey { get; set; }

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


        public FileRowStatus FileRowType { get; set; } = FileRowStatus.NewUpload;
        [Column(TypeName = "varchar(350)")]
        public string? ErrorMessage { get; set; }

        public int PriorityId { get; set; }

        public int FileJobId { get; set; }
        public FileJob FileJob { get; set; }
    }
}
