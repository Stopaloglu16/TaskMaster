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


        public async Task<IReadOnlyList<int>> GetFileJobListRunning(CancellationToken cancellationToken)
        {
            return await _dbContext.FileJobs
                                   .AsNoTracking()
                                   .Where(qq => qq.FileJobType == Domain.Enums.FileJobType.Running &&
                                                      qq.IsCompleted == false)
                                   .Select(ss => ss.Id)
                                   .ToListAsync(cancellationToken);
        }

        public async Task<int> CompleteFileJobAsync(int fileJobId, CancellationToken cancellationToken = default)
        {

            var fileJob = await _dbContext.FileJobs
                                          .FirstOrDefaultAsync(qq => qq.Id == fileJobId, cancellationToken);
            if (fileJob == null)
            {
                return 0;
            }
            fileJob.FileJobType = Domain.Enums.FileJobType.MovedToLive;
            fileJob.IsCompleted = true;

            _dbContext.FileJobs.Update(fileJob);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }


        public async Task<int> MoveToLiveAsync(int fileJobId, CancellationToken cancellationToken)
        {

            return await _dbContext.Database.ExecuteSqlRawAsync(@"
                                        -- Step 1: Insert Tasks
                                        CREATE TABLE #TempTasks (
                                            OldId INT,
                                            Title NVARCHAR(255),
                                            DueDate DATETIME,
                                            AssignedToId INT
                                        );

                                        -- Populate temporary table with source data
                                        INSERT INTO #TempTasks (OldId, Title, DueDate, AssignedToId)
                                        SELECT Id, TaskTitle, DueDate, AssignedToId
                                        FROM FileJobUploads
                                        WHERE IsDeleted = 0 AND [FileRowType] = 2 AND FileJobId = {0};

                                        -- Step 2: Insert into Tasks table
                                        INSERT INTO [TaskLists] (Title, DueDate, [CompletedDate], AssignedToId, IsCompleted, IsDeleted, Created, CreatedBy)
                                        SELECT Title, DueDate, NULL, AssignedToId, 0, 0, GETDATE(), 'fileupload'
                                        FROM #TempTasks;

                                        -- Step 3: Get the mapping between old IDs and new IDs
                                        -- Use a CTE to map old -> new IDs based on matching Title + DueDate + AssignedToId
                                        WITH TaskMapping AS (
                                            SELECT t.Id AS NewId, tmp.OldId
                                            FROM [TaskLists] t
                                            JOIN #TempTasks tmp
                                              ON t.Title = tmp.Title
                                             AND t.DueDate = tmp.DueDate
                                             AND t.AssignedToId = tmp.AssignedToId
                                        )
                                        -- Step 4: Insert TaskDetails using the mapping
                                        INSERT INTO [TaskItems] (TaskListId, Title, Description,IsDeleted,IsCompleted)
                                        SELECT m.NewId, f.Title, f.Description,0, 0
                                        FROM FileJobUploads f
                                        JOIN TaskMapping m
                                          ON f.Id = m.OldId
                                        WHERE f.IsDeleted = 0;

                                        UPDATE FileJobUploads
                                        SET FileRowType = 5
                                        WHERE IsDeleted = 0 AND [FileRowType] = 2 AND FileJobId = {0};


                                        -- Clean up temp table
                                        DROP TABLE #TempTasks;

                                    ", fileJobId);

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
