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

    public async Task<CustomResult> CompleteTaskItem(CompleteTaskItemRequest completeTaskItemRequest)
    {
        var currentTaskItem = await _taskItemRepository.GetByIdAsync(completeTaskItemRequest.Id);

        currentTaskItem.CompletedDate = DateOnly.FromDateTime(DateTime.Now);

        return await _taskItemRepository.UpdateAsync(currentTaskItem);
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

    public async Task<CustomResult> UpdateTaskItem(int Id, UpdateTaskItemRequest updateTaskItemRequest)
    {
        var currentTaskItem = await _taskItemRepository.GetByIdAsync(Id);

        if (currentTaskItem == null) return CustomResult.Failure("Task Item not found");

        var validation = await CheckMaxTaskItemPerTaskList((int)currentTaskItem.TaskListId);
        if (!validation.IsSuccess) return validation;

        currentTaskItem.Title = updateTaskItemRequest.Title;
        currentTaskItem.Description = updateTaskItemRequest.Description;

        return await _taskItemRepository.UpdateAsync(currentTaskItem);
    }


    public async Task<CustomResult> SoftDeleteTaskItemById(int Id)
    {
        var currentTaskItem = await _taskItemRepository.GetByIdAsync(Id);

        currentTaskItem.IsDeleted = 1;

        return await _taskItemRepository.UpdateAsync(currentTaskItem);
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

    public async Task<IEnumerable<TaskItemDto>> GetTaskItemsByTaskItem(int taskListId, CancellationToken cancellationToken)
    {
        return await _taskItemRepository.GetTaskItems(taskListId, cancellationToken);
    }

}
