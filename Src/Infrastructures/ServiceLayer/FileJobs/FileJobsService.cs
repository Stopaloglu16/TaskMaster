using Application.Aggregates.FileJobAggregate.Commands;
using Application.Aggregates.FileJobAggregate.Queries;
using Application.Common.Models;
using Application.Messaging.Contracts;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ServiceLayer.FileJobs
{
    public class FileJobsService : IFileJobService
    {
        private readonly IFileJobRepository _fileJobRepository;
        private readonly IFileJobUploadRepository _fileJobUploadRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<FileJobsService> _logger;

        public FileJobsService(IFileJobRepository fileJobRepository,
                               IFileJobUploadRepository fileJobUploadRepository,
                               ApplicationDbContext dbContext,
                               ILogger<FileJobsService> logger)
        {
            _fileJobRepository = fileJobRepository;
            _fileJobUploadRepository = fileJobUploadRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<CustomResult<int>> CreateFileJob(int FileJobId, List<CreateFileJobUploadRequest> createFileJobUploadRequestList, CancellationToken cancellationToken)
        {
            if (FileJobId == 0)
            {
                var NewFileJob = await _fileJobRepository.AddAsync(new FileJob()
                {
                    IsCompleted = false,
                    State = SagaState.NewUpload
                });

                FileJobId = NewFileJob.Id;
            }

            var fileJobUploads = createFileJobUploadRequestList.Select(request => new FileJobUpload
            {
                FileJobId = FileJobId,
                BatchKey = request.BatchKey,
                TaskTitle = request.TaskTitle,
                DueDate = request.DueDate,
                Title = request.Title,
                Description = request.Description,
                AssignedTo = request.AssignedTo,
                FileRowType = FileRowStatus.NewUpload
            }).ToList();

            await _fileJobUploadRepository.AddRangeAsync(fileJobUploads);

            return CustomResult<int>.Success(FileJobId);
        }

        public async Task<PagingResponse<FileJobUploadDto>> GetFileJobUploadsWithPagination(int FileJobId, PagingParameters pagingParameters, CancellationToken cancellationToken)
        {
            return await _fileJobUploadRepository.GetFileJobUploadsWithPagination(FileJobId, pagingParameters, cancellationToken);
        }

        public async Task<CustomResult> StartFileJobSaga(int FileJobId, CancellationToken cancellationToken)
        {
            var fileJob = await _dbContext.FileJobs
                                          .FirstOrDefaultAsync(f => f.Id == FileJobId, cancellationToken);

            if (fileJob is null)
            {
                // develop's per-row validation loop used to live here. It is now saga step 1
                // (ValidateFileHandler), batched and inside the dispatcher's transaction.
                return CustomResult.Failure($"File job {FileJobId} was not found.");
            }

            // Starting is only meaningful from NewUpload. Re-posting /Process for a job that is
            // already running would otherwise enqueue a second ValidateFile and race the saga.
            if (fileJob.State != SagaState.NewUpload)
            {
                return CustomResult.Failure($"File job {FileJobId} has already been started (state: {fileJob.State}).");
            }

            fileJob.State = SagaState.Started;
            fileJob.Version++;

            // The state change and the outbox row commit together: the saga can never be marked
            // started without its first command being publishable, or vice versa.
            _dbContext.Enqueue(new ValidateFile(Guid.NewGuid(), fileJob.CorrelationId, fileJob.Id));

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("FileJob {FileJobId}: saga started ({CorrelationId}).",
                fileJob.Id, fileJob.CorrelationId);

            return CustomResult.Success();
        }

        public async Task<Dictionary<string, int>> GetFileJobUploadsGroupedByRowType(int FileJobId, CancellationToken cancellationToken)
        {
            return await _fileJobRepository.GetFileJobUploadsGroupedByRowType(FileJobId, cancellationToken);
        }
    }
}
