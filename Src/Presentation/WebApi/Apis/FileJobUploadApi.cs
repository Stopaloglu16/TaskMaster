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

            // Routes for modify
            group.MapPost("/Bulk", CreateFileJob);
            group.MapPatch("/Validate/{FileJobId:int}", ValidateFileJob);

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


        public static async Task<Results<Ok, BadRequest<string>>> ValidateFileJob(int FileJobId,
                                                                                   IFileJobService fileJobService,
                                                                                   CancellationToken cancellationToken)
        {
            var customResult = await fileJobService.ValidateFileJob(FileJobId, cancellationToken);

            if (customResult.IsSuccess)
            {
                return TypedResults.Ok();
            }
            else
            {
                return TypedResults.BadRequest(customResult.Error);
            }
        }
        #endregion
    }
}
