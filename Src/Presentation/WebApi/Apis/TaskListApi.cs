using Application.Aggregates.TaskListAggregate.Commands.Create;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.TaskLists;

namespace WebApi.Apis
{
    public static class TaskListApi
    {

        public static RouteGroupBuilder TaskListApiV1(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/v{apiVersion:apiVersion}/tasklist")
                                        .HasApiVersion(1.0);

            api.MapGet("/", GetTaskListList);
            api.MapPost("/", CreateTaskList);

            return api;
        }


        public static async Task<Ok<IEnumerable<TaskListDto>>> GetTaskListList(ITaskListService taskListService, 
                                                                               CancellationToken cancellationToken)
        {
            var taskList = await taskListService.GetTaskLists(cancellationToken);

            return TypedResults.Ok(taskList.AsEnumerable());
        }


        public static async Task<Results<Created,BadRequest<string>>> CreateTaskList(CreateTaskListRequest createTaskListRequest,
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


    }
}
