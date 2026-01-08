using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskItemAggregate.Commands.Update;
using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;

namespace ServiceLayer.TaskItems;

public class TaskItemService : ITaskItemService
{
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly ITaskListRepository _taskListRepository;

    public TaskItemService(ITaskItemRepository taskItemRepository, ITaskListRepository taskListRepository)
    {
        _taskItemRepository = taskItemRepository;
        _taskListRepository = taskListRepository;
    }

    public async Task<CustomResult> CompleteSingleTaskItem(CompleteTaskItemRequest completeTaskItemRequest, CancellationToken cancellationToken)
    {
        return await _taskItemRepository.CompleteSingleTaskItem(completeTaskItemRequest.taskItemId, cancellationToken);
    }

    public async Task<CustomResult> CreateTaskItem(TaskItemFormRequest taskItemFormRequest)
    {

        var taskList = await _taskListRepository.GetByIdAsync((int)taskItemFormRequest.TaskListId);

        if (taskList == null) return CustomResult.Failure("TaskList not found");

        var validation = await CheckMaxTaskItemPerTaskList((int)taskItemFormRequest.TaskListId);

        if (!validation.IsSuccess) return validation;

        TaskItem newTaskItem = new TaskItem()
        {
            Title = taskItemFormRequest.Title,
            Description = taskItemFormRequest.Description,
            TaskListId = taskItemFormRequest.TaskListId
        };

        var newTaskItemRepo = await _taskItemRepository.AddAsync(newTaskItem);

        if (newTaskItemRepo == null)
            return CustomResult.Failure("Not saved");

        return CustomResult.Success();
    }

    public async Task<CustomResult> UpdateTaskItem(int Id, TaskItemFormRequest taskItemFormRequest)
    {
        var currentTaskItem = await _taskItemRepository.GetByIdAsync(Id);

        if (currentTaskItem == null) return CustomResult.Failure("Task Item not found");

        var validation = await CheckMaxTaskItemPerTaskList((int)currentTaskItem.TaskListId);
        if (!validation.IsSuccess) return validation;

        currentTaskItem.Title = taskItemFormRequest.Title;
        currentTaskItem.Description = taskItemFormRequest.Description;

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



    public async Task<CustomResult<TaskItemFormRequest>> GetTaskItemId(int Id, CancellationToken cancellationToken)
    {
        var taskItemFormRequest = await _taskItemRepository.GetTaskItemById(Id, cancellationToken);

        if (taskItemFormRequest == null) return CustomResult<TaskItemFormRequest>.Failure(new CustomError(false, "Not found"));

        return CustomResult<TaskItemFormRequest>.Success(taskItemFormRequest);
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
