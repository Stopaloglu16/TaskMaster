using Application.Aggregates.FileJobAggregate.Queries;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FileJobRepository : EfCoreRepository<FileJob, int>, IFileJobRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public FileJobRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<FileJobUpload>> GetFileJobUploads(int FileJobId, CancellationToken cancellationToken)
        {
            return await _dbContext.FileJobUploads.Where(qq => qq.FileJobId == FileJobId)
                                          .AsNoTracking()
                                          .ToListAsync();
        }

        public async Task<PagingResponse<FileJobUploadDto>> GetFileJobUploadsWithPagination(int FileJobId, PagingParameters pagingParameters, CancellationToken cancellationToken)
        {
            var query = _dbContext.FileJobUploads.Where(qq => qq.FileJobId == FileJobId)
                                       .AsNoTracking()
                                       .Select(ss => ss.MapToDto());

            var rtnn = await PagingResponse<FileJobUploadDto>.CreateAsync(query, pagingParameters, cancellationToken);

            return rtnn;
        }
    }
}
