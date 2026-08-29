using Application.Aggregates.FileJobAggregate.Commands;
using Application.Aggregates.FileJobAggregate.Queries;
using Application.Common.Models;

namespace ServiceLayer.FileJobs
{
    public interface IFileJobService
    {
        Task<PagingResponse<FileJobUploadDto>> GetFileJobUploadsWithPagination(int FileJobId, PagingParameters pagingParameters, CancellationToken cancellationToken);

        Task<CustomResult<int>> CreateFileJob(int FileJobId, List<CreateFileJobUploadRequest> createFileJobUploadRequestList, CancellationToken cancellationToken);

        /// <summary>
        /// Starts the import saga: moves the job to Started and enqueues ValidateFile in one
        /// transaction. Validation, processing and promotion all happen in the worker from here.
        /// </summary>
        Task<CustomResult> StartFileJobSaga(int FileJobId, CancellationToken cancellationToken);

        Task<Dictionary<string, int>> GetFileJobUploadsGroupedByRowType(int FileJobId, CancellationToken cancellationToken);
    }
}
