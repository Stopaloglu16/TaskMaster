using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Aggregates.FileJobAggregate.Queries
{
    public class FileJobUploadDto
    {
        public int Id { get; set; }

        public string TaskTitle { get; set; } = string.Empty;
        public DateOnly? DueDate { get; set; }
        public int? AssignedToId { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string? AssignedTo { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }


        public string FileRowStatus { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }

    }
}
