using Application.Aggregates.FileJobAggregate.Commands;
using Application.Aggregates.FileJobAggregate.Queries;
using Application.Common.Models;

namespace ServiceLayer.FileJobs
{
    public interface IFileJobService
    {
        Task<PagingResponse<FileJobUploadDto>> GetFileJobUploadsWithPagination(int FileJobId, PagingParameters pagingParameters, CancellationToken cancellationToken);

        Task<CustomResult<int>> CreateFileJob(int FileJobId, List<CreateFileJobUploadRequest> createFileJobUploadRequestList, CancellationToken cancellationToken);
        Task<CustomResult> ValidateFileJob(int FileJobId, CancellationToken cancellationToken);


    }
}
