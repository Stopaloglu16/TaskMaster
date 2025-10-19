using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class FileJob : BaseEntity<int>
    {

        public bool IsCompleted { get; set; } = false;

        public FileJobType FileJobType { get; set; } = FileJobType.NewUpload;

        public virtual IList<FileJobUpload> FileJobUploads { get; private set; } = new List<FileJobUpload>();
    }
}
