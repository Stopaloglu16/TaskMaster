using Application.Aggregates.FileJobAggregate.Commands;
using Application.Aggregates.FileJobAggregate.Queries;
using Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.FileJobs;

namespace WebApi.Apis
{
    public static class FileJobUploadApi
    {
        public static RouteGroupBuilder FileUploadApiV1(this RouteGroupBuilder group)
        {
            // Route for query task lists
            group.MapGet("/", GetFileJobUploadsWithPagination);

            group.MapGet("/GetStatusOfJob/{id:int}", GetFileJobUploadsGroupedByRowType);

            // Routes for modify
            group.MapPost("/Bulk", CreateFileJob);

            // Starts the import saga. Validation, assignee resolution and promotion all follow
            // automatically in the worker, so there is no separate /Validate step any more.
            group.MapPatch("/Process/{FileJobId:int}", StartFileJobSaga);

            return group;
        }


        public static async Task<Ok<PagingResponse<FileJobUploadDto>>> GetFileJobUploadsWithPagination(int FileJobId,
                                                                                                        [AsParameters] PagingParameters pagingParameters,
                                                                                                        [FromServices] IFileJobService fileJobService,
                                                                                                        CancellationToken cancellationToken)
        {

            var taskList = await fileJobService.GetFileJobUploadsWithPagination(FileJobId, pagingParameters, cancellationToken);
            return TypedResults.Ok(taskList);
        }

        public static async Task<Ok<Dictionary<string, int>>> GetFileJobUploadsGroupedByRowType(int id, IFileJobService fileJobService,
                                                                                                       CancellationToken cancellationToken)
        {
            return TypedResults.Ok(await fileJobService.GetFileJobUploadsGroupedByRowType(id, cancellationToken));
        }
        #region API Routes for modify

        public static async Task<Results<Created<int>, BadRequest<string>>> CreateFileJob(int FileJobId,
                                                                                     List<CreateFileJobUploadRequest> createFileJobUploadRequestList,
                                                                                     IFileJobService fileJobService,
                                                                                     CancellationToken cancellationToken)
        {
            var customResult = await fileJobService.CreateFileJob(FileJobId, createFileJobUploadRequestList, cancellationToken);

            if (customResult.IsSuccess)
            {
                var tt = customResult.Value;
                return TypedResults.Created($"/items/customResult.Value", customResult.Value);
            }
            else
            {
                return TypedResults.BadRequest(customResult.IsFailure.ToString());
            }
        }


        /// <summary>
        /// Starts the import saga. Returns as soon as the first command is in the outbox — the work
        /// itself happens in the worker, so poll GetStatusOfJob for progress.
        /// </summary>
        public static async Task<Results<Accepted, BadRequest<string>>> StartFileJobSaga(int FileJobId,
                                                                                          IFileJobService fileJobService,
                                                                                          CancellationToken cancellationToken)
        {
            var customResult = await fileJobService.StartFileJobSaga(FileJobId, cancellationToken);

            if (customResult.IsSuccess)
            {
                return TypedResults.Accepted($"/api/v1/fileupload/GetStatusOfJob/{FileJobId}");
            }
            else
            {
                return TypedResults.BadRequest(customResult.Error);
            }
        }

        #endregion
    }
}
