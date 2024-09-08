using Application.Aggregates.TaskItemAggregate.Commands.Create;
using Application.Aggregates.TaskItemAggregate.Commands.Update;
using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;

namespace ServiceLayer.TaskItems;

public class TaskItemService : ITaskItemService
{
    private readonly ITaskItemRepository _taskItemRepository;

    public TaskItemService(ITaskItemRepository taskItemRepository)
    {
        _taskItemRepository = taskItemRepository;
    }

    public Task<CustomResult> CompleteTaskItem(CompleteTaskItemRequest completeTaskItemRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<CustomResult> CreateTaskItem(CreateTaskItemRequest createTaskItemRequest)
    {
        var validation = await CheckMaxTaskItemPerTaskList((int)createTaskItemRequest.TaskListId);

        if (!validation.IsSuccess) return validation;


        TaskItem newTaskItem = new TaskItem()
        {
            Title = createTaskItemRequest.Title,
            Description = createTaskItemRequest.Description,
            TaskListId = createTaskItemRequest.TaskListId
        };

        var newTaskItemRepo = await _taskItemRepository.AddAsync(newTaskItem);

        if (newTaskItemRepo == null)
            return CustomResult.Failure("Not saved");

        return CustomResult.Success();
    }

    public async Task<CustomResult> UpdateTaskItem(UpdateTaskItemRequest updateTaskItemRequest)
    {
        var validation = await CheckMaxTaskItemPerTaskList((int)updateTaskItemRequest.TaskListId);
        if (!validation.IsSuccess) return validation;

        var currentTaskList = await _taskItemRepository.GetByIdAsync(updateTaskItemRequest.Id);

        currentTaskList.Title = updateTaskItemRequest.Title;
        currentTaskList.Title = updateTaskItemRequest.Description;

        return await _taskItemRepository.UpdateAsync(currentTaskList);
    }


    public async Task<CustomResult> SoftDeleteTaskListById(int Id)
    {
        return await _taskItemRepository.DeleteAsync(Id);
    }




    /// <summary>
    /// Check the user assigned enough task
    /// </summary>
    /// <param name="Id">Task user Id</param>
    /// <returns></returns>
    protected async Task<CustomResult> CheckMaxTaskItemPerTaskList(int Id)
    {
        var taskItemCount = await _taskItemRepository.CheckMaxTaskItemPerTaskList(Id);

        if (taskItemCount.Value == 50)
            return CustomResult.Failure("The task list reached to max task item");

        return CustomResult.Success();
    }



    public Task<TaskItemDto> GetTaskItemId(int Id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SelectListItem>> GetTaskItemList()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TaskItemDto>> GetTaskItems(int taskListId)
    {
        throw new NotImplementedException();
    }
}
