using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.TaskLists;
using ServiceLayer.Users;

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

            api.MapGet("/GetTaskListForm/{id:int}", GetTaskListForm);
            api.MapGet("/GetTaskList/{id:int}", GetTaskList);

            //TODO: Get tasklist assigned to user
            api.MapGet("/TaskListwithItemsByUserId/{aspUserId}", GetTaskListWithItemsByUser);


            //TODO: Add paging (search page) 

            // Routes for modify
            api.MapPost("/", CreateTaskList);
            api.MapPut("/{id:int}", UpdateTaskList);
            api.MapDelete("/{id:int}", DeleteTaskList);

            //TODO: Assign to multi user
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

        public static async Task<Results<Ok<TaskListDto>, BadRequest<CustomError>>> GetTaskList(int id, ITaskListService taskListService,
                                                                                                        CancellationToken cancellationToken)
        {
            var taskListFormRequest = await taskListService.GetTaskListById(id, cancellationToken);

            if (taskListFormRequest.IsSuccess)
            {
                return TypedResults.Ok(taskListFormRequest.Value);
            }
            else
            {
                return TypedResults.BadRequest(taskListFormRequest.CustomError);
            }
        }


        public static async Task<Results<Ok<TaskListFormRequest>, BadRequest<CustomError>>> GetTaskListForm(int id, ITaskListService taskListService,
                                                                                                    CancellationToken cancellationToken)
        {
            var taskListFormRequest = await taskListService.GetTaskListFormById(id, cancellationToken);

            if (taskListFormRequest.IsSuccess)
            {
                return TypedResults.Ok(taskListFormRequest.Value);
            }
            else
            {
                return TypedResults.BadRequest(taskListFormRequest.CustomError);
            }
        }

        public static async Task<Results<Ok<IEnumerable<TaskListWithItemsDto>>, BadRequest<string>>> GetTaskListWithItemsByUser(string aspUserId,
                                                                                                   [FromServices] ITaskListService taskListService,
                                                                                                   [FromServices] IUserService userService,
                                                                                                   CancellationToken cancellationToken)
        {
            var userDto = await userService.GetUserByAspId(aspUserId);

            if (userDto.IsFailure)
            {
                return TypedResults.BadRequest("User not found");
            }

            var taskItem = await taskListService.GetTaskListWithItemsByUser(userDto.Value.Id, cancellationToken);

            return TypedResults.Ok(taskItem.AsEnumerable());
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

        public static async Task<Results<Ok, BadRequest<string>>> UpdateTaskList(int id, TaskListFormRequest taskListFormRequest,
                                                                                 ITaskListService taskListService)
        {
            var customResult = await taskListService.UpdateTaskList(id, taskListFormRequest);

            if (customResult.IsSuccess)
            {
                return TypedResults.Ok();
            }
            else
            {
                return TypedResults.BadRequest(customResult.Error);
            }
        }

        public static async Task<Results<NoContent, BadRequest<string>>> DeleteTaskList(int id, ITaskListService taskListService)
        {
            var customResult = await taskListService.SoftDeleteTaskListById(id);

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
