using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IFileJobRepository : IRepository<FileJob, int>
    {
        Task<List<FileJobUpload>> GetFileJobUploads(int FileJobId, CancellationToken cancellationToken);

        Task<Dictionary<string, int>> GetFileJobUploadsGroupedByRowType(int FileJobId, CancellationToken cancellationToken);

        // GetFileJobListRunning / CompleteFileJobAsync / MoveToLiveAsync are gone: the saga drives
        // the pipeline by message now, so there is nothing to poll for and no separate "complete"
        // step that could commit ahead of the promotion. See ServiceLayer/FileJobs/Saga.
    }
}
