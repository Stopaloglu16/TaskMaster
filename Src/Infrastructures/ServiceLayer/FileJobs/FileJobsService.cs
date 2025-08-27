using Application.Aggregates.FileJobAggregate.Commands;
using Application.Aggregates.FileJobAggregate.Queries;
using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using Microsoft.Extensions.Logging;
using ServiceLayer.TaskLists;
using ServiceLayer.Users;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ServiceLayer.FileJobs
{
    public class FileJobsService : IFileJobService
    {

        private readonly IFileJobRepository _fileJobRepository;
        private readonly IFileJobUploadRepository _fileJobUploadRepository;
        private readonly IUserService _userService;
        private readonly ILogger<TaskListService> _logger;

        public FileJobsService(IFileJobRepository fileJobRepository,
                               IFileJobUploadRepository fileJobUploadRepository,
                               IUserService userService,
                               ILogger<TaskListService> logger)
        {
            _fileJobRepository = fileJobRepository;
            _fileJobUploadRepository = fileJobUploadRepository;
            _userService = userService;
            _logger = logger;
        }

        public async Task<CustomResult<int>> CreateFileJob(int FileJobId, List<CreateFileJobUploadRequest> createFileJobUploadRequestList, CancellationToken cancellationToken)
        {

            if (FileJobId == 0)
            {
                var NewFileJob = await _fileJobRepository.AddAsync(new FileJob()
                {
                    IsCompleted = false,
                    FileJobType = FileJobType.NewUpload
                });

                FileJobId = NewFileJob.Id;
            }



            var fileJobUploads = createFileJobUploadRequestList.Select(request => new FileJobUpload
            {
                FileJobId = FileJobId,
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
            return await _fileJobRepository.GetFileJobUploadsWithPagination(FileJobId, pagingParameters, cancellationToken);
        }

        public async Task<CustomResult> ValidateFileJob(int FileJobId, CancellationToken cancellationToken)
        {
            try
            {
                var fileJobList = await _fileJobRepository.GetFileJobUploads(FileJobId, cancellationToken);


                foreach (var fileJob in fileJobList)
                {
                    StringBuilder errors = new StringBuilder();

                    CreateTaskListRequest createTaskListRequest = new CreateTaskListRequest()
                    {
                        Title = fileJob.TaskTitle,
                        DueDate = fileJob.DueDate,
                        AssignedTo = fileJob.AssignedTo
                    };

                    // Validate the CreateTaskListRequest
                    var validationContext = new ValidationContext(createTaskListRequest);
                    var validationResults = new List<ValidationResult>();

                    if (!Validator.TryValidateObject(createTaskListRequest, validationContext, validationResults, validateAllProperties: true))
                    {
                        // Log validation errors
                        _logger.LogWarning("Validation failed for TaskList with Title: {Title}", fileJob.TaskTitle);

                        // Collect all validation errors for this request
                        foreach (var validationResult in validationResults)
                        {
                            errors.Append(validationResult.ErrorMessage);
                        }
                        continue; // Skip to next item if current one has validation errors
                    }

                    CreateTaskItemRequest createTaskItemRequest = new CreateTaskItemRequest()
                    {
                        Title = fileJob.Title,
                        Description = fileJob.Description
                    };

                    // Validate the CreateTaskItemRequest
                    validationContext = new ValidationContext(createTaskItemRequest);
                    validationResults.Clear();

                    if (!Validator.TryValidateObject(createTaskItemRequest, validationContext, validationResults, validateAllProperties: true))
                    {
                        foreach (var validationResult in validationResults)
                        {
                            errors.Append(validationResult.ErrorMessage);
                        }
                    }

                    //fileJob.Id
                    if(errors.Length == 0)
                    {
                        fileJob.FileRowType = FileRowStatus.Validated;
                    }
                    else
                    {
                        fileJob.ErrorMessage = errors.ToString();
                        fileJob.FileRowType = FileRowStatus.ValidateIssue;
                    }

                    await _fileJobUploadRepository.UpdateAsync(fileJob);

                }

                return CustomResult.Success();
            }
            catch (Exception)
            {
                return CustomResult.Failure("Error in validating the file job");
            }

        }
    }
}
