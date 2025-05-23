﻿using Application.Aggregates.TaskItemAggregate.Commands.Create;
using Application.Aggregates.TaskItemAggregate.Commands.Update;
using Application.Aggregates.TaskItemAggregate.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using ServiceLayer.TaskItems;

namespace WebApi.Apis
{
    public static class TaskItemApi
    {
        public static RouteGroupBuilder TaskItemApiV1(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/v{apiVersion:apiVersion}/taskitem")
                                        .HasApiVersion(1.0);

            // Route for query task items
            api.MapGet("/", GetTaskItemActive);

            //TODO: Get taskItem assigned to user

            //TODO: Add paging (search page) 


            // Routes for modify
            api.MapPost("/", CreateTaskItem);
            api.MapPut("/{id:int}", UpdateTaskItem);
            api.MapDelete("/{id:int}", DeleteTaskItem);

            //TODO: Assing to multi user
            //api.MapPatch("/{id}", AssignTaskItemToUser);

            return api;
        }


        public static async Task<Ok<IEnumerable<TaskItemDto>>> GetTaskItemActive(int taskItemId,
                                                                                ITaskItemService taskItemService,
                                                                               CancellationToken cancellationToken)
        {
            var taskItem = await taskItemService.GetTaskItemsByTaskItem(taskItemId, cancellationToken);

            return TypedResults.Ok(taskItem.AsEnumerable());
        }




        #region Routes for modify

        public static async Task<Results<Created, BadRequest<string>>> CreateTaskItem(CreateTaskItemRequest createTaskItemRequest,
                                                                                     ITaskItemService taskItemService)
        {
            var customResult = await taskItemService.CreateTaskItem(createTaskItemRequest);

            if (customResult.IsSuccess)
            {
                return TypedResults.Created();
            }
            else
            {
                return TypedResults.BadRequest(customResult.Error);
            }
        }


        public static async Task<Results<Ok, BadRequest<string>>> UpdateTaskItem(int Id, UpdateTaskItemRequest updateTaskItemRequest,
                                                                                 ITaskItemService taskItemService)
        {
            var customResult = await taskItemService.UpdateTaskItem(Id, updateTaskItemRequest);

            if (customResult.IsSuccess)
            {
                return TypedResults.Ok();
            }
            else
            {
                return TypedResults.BadRequest(customResult.Error);
            }
        }


        public static async Task<Results<NoContent, BadRequest<string>>> DeleteTaskItem(ITaskItemService taskItemService,
                                                                                        int Id)
        {
            var customResult = await taskItemService.SoftDeleteTaskItemById(Id);

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
