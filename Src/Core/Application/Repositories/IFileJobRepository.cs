using Application.Aggregates.FileJobAggregate.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IFileJobRepository : IRepository<FileJob, int>
    {
        Task<PagingResponse<FileJobUploadDto>> GetFileJobUploadsWithPagination(int FileJobId, PagingParameters pagingParameters, CancellationToken cancellationToken);

        Task<List<FileJobUpload>> GetFileJobUploads(int FileJobId, CancellationToken cancellationToken);

    }
}
