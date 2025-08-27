
using Application.Aggregates.FileJobAggregate.Queries;
using Application.Aggregates.TaskListAggregate.Queries;
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

        //public async Task<PagingResponse<FileJobUploadDto>> GetFileJobUploadsWithPagination(int FileJobId, PagingParameters pagingParameters, CancellationToken cancellationToken)
        //{
        //    var query = _dbContext.FileJobUploads.Where(qq => qq.FileJobId == FileJobId)
        //                               .AsNoTracking()
        //                               .Select(ss => ss.MapToDto());

        //    return await PagingResponse<FileJobUploadDto>.CreateAsync(query, pagingParameters, cancellationToken);
        //}
    }
}
