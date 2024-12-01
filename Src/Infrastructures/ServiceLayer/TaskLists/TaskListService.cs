using Application.Aggregates.TaskListAggregate.Commands.Create;
using Application.Aggregates.TaskListAggregate.Commands.Update;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;

namespace ServiceLayer.TaskLists;

public class TaskListService : ITaskListService
{
    private readonly ITaskListRepository _taskListRepository;

    public TaskListService(ITaskListRepository taskListRepository)
    {
        _taskListRepository = taskListRepository;
    }

    #region Crud operations

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

        var newTaskListRepo = await _taskListRepository.AddAsync(newTaskList);

        if (newTaskListRepo == null) return CustomResult.Failure("Not created");

        return CustomResult.Success();
    }

    public async Task<CustomResult> UpdateTaskList(int Id, UpdateTaskListRequest updateTaskListRequest)
    {
        if (updateTaskListRequest.AssignedToId > 0)
        {
            var validation = await CheckMaxTaskListPerUser((int)updateTaskListRequest.AssignedToId);

            if (!validation.IsSuccess) return validation;
        }

        var currentTaskList = await _taskListRepository.GetByIdAsync(Id);

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

    #endregion

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


    public async Task<PagingResponse<TaskListDto>> GetActiveTaskListWithPagination(PagingParameters pagingParameters, CancellationToken cancellationToken)
    {
        return await _taskListRepository.GetActiveTaskListWithPagination(pagingParameters, cancellationToken);
    }

    public Task<TaskListDto> GetTaskListId(int Id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SelectListItem>> GetTaskListSelectList()
    {
        throw new NotImplementedException();
    }

}
