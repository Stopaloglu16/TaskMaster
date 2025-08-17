using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.TaskLists;
using ServiceLayer.Users;
using WebApi.Notification;
using WebApi.RabbitMq;

namespace WebApi.Apis
{
    public static class TaskListApi
    {

        public static RouteGroupBuilder TaskListApiV1(this RouteGroupBuilder group)
        {

            // Route for query task lists
            group.MapGet("/", GetActiveTaskListWithPagination);

            group.MapGet("/GetTaskListForm/{id:int}", GetTaskListForm);
            group.MapGet("/GetTaskList/{id:int}", GetTaskList);

            //TODO: Get tasklist assigned to user
            group.MapGet("/TaskListwithItemsByUserId/{aspUserId}", GetTaskListWithItemsByUser);


            //TODO: Add paging (search page) 

            // Routes for modify
            group.MapPost("/", CreateTaskList);
            group.MapPost("/Bulk", CreateTaskListBulk).MapToApiVersion(1.0);
            group.MapPost("/Bulk", CreateTaskListBulkV2).MapToApiVersion(2.0);
            group.MapPost("/Bulk1/", CreateTaskListBulkRabbitMq).MapToApiVersion(1.0);
            group.MapGet("/GetProcessrabbitMq", GetProcessrabbitMq).MapToApiVersion(1.0); ;
            group.MapPut("/{id:int}", UpdateTaskList);
            group.MapDelete("/{id:int}", DeleteTaskList);

            //TODO: Assign to multi user
            //api.MapPatch("/{id}", AssignTaskListToUser);
            
            return group;
        }

        // Change ILogger<TaskListApi> to ILogger in GetActiveTaskListWithPagination method signature and usage

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

        #region API Routes for modify

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


        #region API Routes bulk Upload

        public static async Task<Results<Ok<List<CreateTaskListResponse>>, BadRequest<string>>> CreateTaskListBulk([FromBody] IEnumerable<CreateTaskListRequest> createTaskListRequests,
                                                                                                                  ITaskListService taskListService,
                                                                                                                  CancellationToken cancellationToken)
        {
            var customResult = await taskListService.CreateTaskListBulk(createTaskListRequests, cancellationToken);
            if (customResult.IsSuccess)
            {
                return TypedResults.Ok(customResult.Value);
            }
            else
            {
                return TypedResults.BadRequest("System issue");
            }
        }


        public static async Task<IResult> CreateTaskListBulkV2([FromBody] IEnumerable<CreateTaskListRequest> createTaskListRequests,
                                                                          IBackgroundTaskQueue backgroundTaskQueue,
                                                                          CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid().ToString();
            backgroundTaskQueue.QueueTask(requestId, createTaskListRequests);

            return Results.Ok(new { RequestId = requestId });
        }

        public static async Task<IResult> CreateTaskListBulkRabbitMq(List<ProcessItem> items,
                                                                     RabbitPublisher publisher,
                                                                     ResultStore store,
                                                                     CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid().ToString();
            store.EnsureRequest(requestId);

            foreach (var item in items)
            {
                publisher.Publish(new ProcessMessage(requestId, item));
            }

            return Results.Ok(new { RequestId = requestId });
        }

        public static async Task<IResult> GetProcessrabbitMq(string requestId,
                                                                                                ResultStore store,
                                                                                                CancellationToken cancellationToken)
        {
            var results = store.Get(requestId);
            return results is null ? Results.NotFound() : Results.Ok(results);
        }

        #endregion



    }
}
