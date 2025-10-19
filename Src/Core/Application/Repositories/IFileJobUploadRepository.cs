using Application.Aggregates.FileJobAggregate.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IFileJobUploadRepository : IRepository<FileJobUpload, int>
    {

        Task<List<FileJobUpload>> GetFileJobUploadList(int fileJobId, CancellationToken cancellationToken);
        Task<PagingResponse<FileJobUploadDto>> GetFileJobUploadsWithPagination(int FileJobId, PagingParameters pagingParameters, CancellationToken cancellationToken);

        Task<int> UpdateFileJobUploadRangeAsync(List<FileJobUpload> fileJobUploads, CancellationToken cancellationToken = default);
    }
}
