using Application.Aggregates.FileJobAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FileJobUploadRepository : EfCoreRepository<FileJobUpload, int>, IFileJobUploadRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public FileJobUploadRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<FileJobUpload>> GetFileJobUploadList(int FileJobId, CancellationToken cancellationToken)
        {
            return await _dbContext.FileJobUploads.Where(qq => qq.FileJobId == FileJobId &&
                                                                    qq.FileRowType == Domain.Enums.FileRowStatus.Validated)
                                                  .AsNoTracking()
                                                  .OrderBy(oo => oo.Id)
                                                  .Take(10)
                                                  .ToListAsync(cancellationToken);
        }

        public async Task<PagingResponse<FileJobUploadDto>> GetFileJobUploadsWithPagination(int FileJobId, PagingParameters pagingParameters, CancellationToken cancellationToken)
        {
            var query = _dbContext.FileJobUploads.Where(qq => qq.FileJobId == FileJobId)
                                       .AsNoTracking()
                                       .Select(ss => ss.MapToDto());

            var rtnn = await PagingResponse<FileJobUploadDto>.CreateAsync(query, pagingParameters, cancellationToken);

            return rtnn;
        }

        // Updates a range of FileJobUpload entities in the database.
        // Returns the number of state entries written to the database.
        public async Task<int> UpdateFileJobUploadRangeAsync(List<FileJobUpload> fileJobUploads, CancellationToken cancellationToken = default)
        {
            if (fileJobUploads == null || fileJobUploads.Count == 0)
                return 0;

            // Mark all provided entities as modified so EF Core will update them.
            _dbContext.FileJobUploads.UpdateRange(fileJobUploads);

            // Persist changes
            var affected = await _dbContext.SaveChangesAsync(cancellationToken);

            return affected;
        }

    }
}
