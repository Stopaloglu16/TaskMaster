using Application.Aggregates.FileJobAggregate.Queries;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IFileJobRepository : IRepository<FileJob, int>
    {

        Task<List<FileJobUpload>> GetFileJobUploads(int FileJobId, CancellationToken cancellationToken);

        Task<IReadOnlyList<int>> GetFileJobListRunning(CancellationToken cancellationToken);

        Task<int> CompleteFileJobAsync(int fileJobId, CancellationToken cancellationToken = default);

        Task<Dictionary<string,int>> GetFileJobUploadsGroupedByRowType(int FileJobId, CancellationToken cancellationToken);

        Task<int> MoveToLiveAsync(int fileJobId, CancellationToken cancellationToken = default);
    }
}
