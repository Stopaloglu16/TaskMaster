using Application.Aggregates.FileJobAggregate.Commands;
using Application.Aggregates.FileJobAggregate.Queries;
using Application.Common.Models;
using Domain.Entities;

namespace ServiceLayer.FileJobs
{
    public interface IFileJobService
    {
        Task<PagingResponse<FileJobUploadDto>> GetFileJobUploadsWithPagination(int FileJobId, PagingParameters pagingParameters, CancellationToken cancellationToken);

        Task<CustomResult<int>> CreateFileJob(int FileJobId, List<CreateFileJobUploadRequest> createFileJobUploadRequestList, CancellationToken cancellationToken);
        Task<CustomResult> ValidateFileJob(int FileJobId, CancellationToken cancellationToken);
        Task<CustomResult> ProcessFileJob(int FileJobId, CancellationToken cancellationToken);


        Task<IReadOnlyList<int>> GetFileJobListRunning(CancellationToken cancellationToken);
        Task<List<FileJobUpload>> GetFileJobUploadList(int fileJobId, CancellationToken cancellationToken);
        Task<int> UpdateFileJobUploadRangeAsync(List<FileJobUpload> fileJobUploads, CancellationToken cancellationToken = default);

        Task<Dictionary<string, int>> GetFileJobUploadsGroupedByRowType(int FileJobId, CancellationToken cancellationToken);

        Task<int> CompleteFileJobAsync(int fileJobId, CancellationToken cancellationToken = default);
        Task<int> MoveToLiveAsync(int fileJobId, CancellationToken cancellationToken = default);
    }
}
