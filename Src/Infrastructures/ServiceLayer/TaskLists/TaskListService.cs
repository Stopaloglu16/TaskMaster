﻿using Application.Aggregates.TaskListAggregate.Commands.Create;
using Application.Aggregates.TaskListAggregate.Commands.Update;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using System.Web.Mvc;

namespace ServiceLayer.TaskLists;

public class TaskListService : ITaskListService
{
    private readonly ITaskListRepository _taskListRepository;

    public TaskListService(ITaskListRepository taskListRepository)
    {
        _taskListRepository = taskListRepository;
    }

    public async Task<CustomResult> CreateTaskList(CreateTaskListRequest createTaskListRequest)
    {
        if (createTaskListRequest.AssignedToId > 0)
        {
            var validation = await CheckMaxTaskListPerUser((int)createTaskListRequest.AssignedToId);

            if (!validation.IsSuccess) return validation;
        }


        TaskList newTaskList = new TaskList()
        {
            Title = createTaskListRequest.Title,
            AssignedToId = createTaskListRequest.AssignedToId,
            DueDate = createTaskListRequest.DueDate
        };

        return await _taskListRepository.AddAsync(newTaskList);
    }

    public async Task<CustomResult> UpdateTaskList(UpdateTaskListRequest updateTaskListRequest)
    {
        if (updateTaskListRequest.AssignedToId > 0)
        {
            var validation = await CheckMaxTaskListPerUser((int)updateTaskListRequest.AssignedToId);

            if (!validation.IsSuccess) return validation;
        }

        var currentTaskList = await _taskListRepository.GetByIdAsync(updateTaskListRequest.Id);

        currentTaskList.Title = updateTaskListRequest.Title;
        currentTaskList.AssignedToId = updateTaskListRequest.AssignedToId;
        currentTaskList.DueDate = updateTaskListRequest.DueDate;

        await _taskListRepository.UpdateAsync(currentTaskList);

        return CustomResult.Success();
    }

    public async Task<CustomResult> SoftDeleteTaskListById(int Id)
    {
        return await _taskListRepository.DeleteAsync(Id);
    }


    /// <summary>
    /// Check the user assigned enough task
    /// </summary>
    /// <param name="Id">Task user Id</param>
    /// <returns></returns>
    protected async Task<CustomResult> CheckMaxTaskListPerUser(int Id)
    {
        var taskListCount = await _taskListRepository.CheckMaxTaskListPerUser(Id);

        if (taskListCount.Value == 10)
            return CustomResult.Failure("THe user reached to max task list");

        return CustomResult.Success();
    }


    public async Task<IEnumerable<TaskListDto>> GetTaskLists()
    {
        return await _taskListRepository.GetTaskListList();
    }

    public Task<TaskListDto> GetTaskListId(int Id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SelectListItem>> GetTaskListList()
    {
        throw new NotImplementedException();
    }

}
