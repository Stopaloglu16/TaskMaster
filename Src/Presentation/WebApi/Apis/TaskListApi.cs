using Application.Aggregates.TaskListAggregate.Commands.Create;
using Application.Aggregates.TaskListAggregate.Commands.Delete;
using Application.Aggregates.TaskListAggregate.Commands.Update;
using Application.Aggregates.TaskListAggregate.Queries;
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
            api.MapGet("/", GetTaskListActive);

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


        public static async Task<Ok<IEnumerable<TaskListDto>>> GetTaskListActive(ITaskListService taskListService,
                                                                               CancellationToken cancellationToken)
        {
            var taskList = await taskListService.GetTaskListActive(cancellationToken);

            return TypedResults.Ok(taskList.AsEnumerable());
        }




        #region Routes for modify

        public static async Task<Results<Created, BadRequest<string>>> CreateTaskList(CreateTaskListRequest createTaskListRequest,
                                                                                     ITaskListService taskListService)
        {
            var customResult = await taskListService.CreateTaskList(createTaskListRequest);

            if (customResult.IsSuccess)
            {
                return TypedResults.Created();
            }
            else
            {
                return TypedResults.BadRequest(customResult.Error);
            }
        }


        public static async Task<Results<Ok, BadRequest<string>>> UpdateTaskList(int Id, UpdateTaskListRequest updateTaskListRequest,
                                                                                 ITaskListService taskListService)
        {
            var customResult = await taskListService.UpdateTaskList(Id, updateTaskListRequest);

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
