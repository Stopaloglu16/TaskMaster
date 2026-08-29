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
                                          .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetFileJobUploadsGroupedByRowType(int FileJobId, CancellationToken cancellationToken)
        {
            var grouped = await _dbContext.FileJobUploads
                .Where(f => f.FileJobId == FileJobId)
                .AsNoTracking()
                .GroupBy(f => (int)f.FileRowType) // cast enum to int for EF translation
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return grouped.ToDictionary(
                x => ((Domain.Enums.FileRowStatus)x.Key).ToString(), // convert int back to enum then to name
                x => x.Count);
        }
    }
}
