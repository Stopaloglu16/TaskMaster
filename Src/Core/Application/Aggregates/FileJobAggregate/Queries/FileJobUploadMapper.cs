using Domain.Entities;

namespace Application.Aggregates.FileJobAggregate.Queries
{
    public static class FileJobUploadMapper
    {
        public static FileJobUploadDto MapToDto(this FileJobUpload fileJobUpload)
        {
            return new FileJobUploadDto
            {
                Id = fileJobUpload.Id,
                TaskTitle = fileJobUpload.TaskTitle,
                AssignedTo = fileJobUpload.AssignedTo,
                DueDate = fileJobUpload.DueDate,
                Title = fileJobUpload.Title,
                Description = fileJobUpload.Description,
                FileRowStatus = fileJobUpload.FileRowType.ToString(),
                ErrorMessage = fileJobUpload.ErrorMessage
            };
        }
    }
}
