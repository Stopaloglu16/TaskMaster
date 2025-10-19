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
            group.MapPatch("/Validate/{FileJobId:int}", ValidateFileJob);
            group.MapPatch("/Process/{FileJobId:int}", ProcessFileJob);

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


        public static async Task<Results<Ok, BadRequest<string>>> ProcessFileJob(int FileJobId,
                                                                                  IFileJobService fileJobService,
                                                                                  CancellationToken cancellationToken)
        {
            var customResult = await fileJobService.ProcessFileJob(FileJobId, cancellationToken);

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
