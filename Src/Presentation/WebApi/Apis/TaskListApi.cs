using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using ServiceLayer.TaskLists;

namespace WebApi.Apis
{
    public static class TaskListApi
    {

        public static RouteGroupBuilder TaskListApiV1(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/v{apiVersion:apiVersion}/tasklist")
                                        .HasApiVersion(1.0);

            // Route for query task lists
            api.MapGet("/", GetActiveTaskListWithPagination);

            api.MapGet("/{id:int}", GetTaskList);

            //TODO: Get tasklist assigned to user

            //TODO: Add paging (search page) 


            // Routes for modify
            api.MapPost("/", CreateTaskList);
            api.MapPut("/{id:int}", UpdateTaskList);
            api.MapDelete("/{id:int}", DeleteTaskList);

            //TODO: Assing to multi user
            //api.MapPatch("/{id}", AssignTaskListToUser);

            return api;
        }


        public static async Task<Ok<PagingResponse<TaskListDto>>> GetActiveTaskListWithPagination(ITaskListService taskListService,
                                                                                                 [AsParameters] PagingParameters pagingParameters,
                                                                                                  CancellationToken cancellationToken)
        {
            var taskList = await taskListService.GetActiveTaskListWithPagination(pagingParameters, cancellationToken);

            return TypedResults.Ok(taskList);
        }


        public static async Task<Results<Ok<TaskListFormRequest>, BadRequest<CustomError>>> GetTaskList(int Id, ITaskListService taskListService,
                                                                                           CancellationToken cancellationToken)
        {
            var taskListFormRequest = await taskListService.GetTaskListById(Id, cancellationToken);

            if (taskListFormRequest.IsSuccess)
            {
                return TypedResults.Ok(taskListFormRequest.Value);
            }
            else
            {
                return TypedResults.BadRequest(taskListFormRequest.CustomError);
            }
        }

        #region Routes for modify

        public static async Task<Results<Created, BadRequest<string>>> CreateTaskList(TaskListFormRequest taskListFormRequest,
                                                                                      ITaskListService taskListService)
        {
            var customResult = await taskListService.CreateTaskList(taskListFormRequest);

            if (customResult.IsSuccess)
            {
                return TypedResults.Created();
            }
            else
            {
                return TypedResults.BadRequest(customResult.Error);
            }
        }


        public static async Task<Results<Ok, BadRequest<string>>> UpdateTaskList(int Id, TaskListFormRequest taskListFormRequest,
                                                                                 ITaskListService taskListService)
        {
            var customResult = await taskListService.UpdateTaskList(Id, taskListFormRequest);

            if (customResult.IsSuccess)
            {
                return TypedResults.Ok();
            }
            else
            {
                return TypedResults.BadRequest(customResult.Error);
            }
        }


        public static async Task<Results<NoContent, BadRequest<string>>> DeleteTaskList(int Id, ITaskListService taskListService)
        {
            var customResult = await taskListService.SoftDeleteTaskListById(Id);

            if (customResult.IsSuccess)
            {
                return TypedResults.NoContent();
            }
            else
            {
                return TypedResults.BadRequest(customResult.Error);
            }
        }

        #endregion

    }
}
