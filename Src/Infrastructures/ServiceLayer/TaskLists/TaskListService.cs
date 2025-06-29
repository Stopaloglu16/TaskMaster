using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using ServiceLayer.Users;

namespace ServiceLayer.TaskLists;

public class TaskListService : ITaskListService
{
    private readonly ITaskListRepository _taskListRepository;
    private readonly IUserService _userService;

    public TaskListService(ITaskListRepository taskListRepository, IUserService userService)
    {
        _taskListRepository = taskListRepository;
        _userService = userService;
    }

   
    #region Crud operations

    public async Task<CustomResult> CreateTaskList(TaskListFormRequest taskListFormRequest)
    {
        if (taskListFormRequest.AssignedToId > 0)
        {
            var validation = await CheckMaxTaskListPerUser((int)taskListFormRequest.AssignedToId);

            if (!validation.IsSuccess) return validation;
        }


        TaskList newTaskList = new TaskList()
        {
            Title = taskListFormRequest.Title,
            AssignedToId = taskListFormRequest.AssignedToId,
            DueDate = taskListFormRequest.DueDate
        };

        var newTaskListRepo = await _taskListRepository.AddAsync(newTaskList);

        if (newTaskListRepo == null) return CustomResult.Failure("Not created");

        return CustomResult.Success();
    }

    public async Task<CustomResult<List<CreateTaskListResponse>>> CreateTaskListBulk(IEnumerable<CreateTaskListRequest> createTaskListRequests,
                                                                                     CancellationToken cancellationToken)
    {
        List<CreateTaskListResponse> createTaskListResponseList = new List<CreateTaskListResponse>();

        try
        {

            //Get users and map to dictionary
            var users =  await _userService.GetUsers(true, Domain.Enums.UserType.TaskUser);
            var userNameToId = users.ToDictionary(u => u.FullName, u => u.Id);
            
            List<TaskList> taskLists = new List<TaskList>();
            CustomError customError;

            foreach (var request in createTaskListRequests)
            {
                customError = new CustomError(true);

                if (!string.IsNullOrEmpty(request.AssignedTo) && !string.IsNullOrWhiteSpace(request.AssignedTo))
                {
                    // Try to get the user ID from the dictionary using the AssignedTo name
                    if (userNameToId.TryGetValue(request.AssignedTo, out var userId))
                    {

                        var validation = await CheckMaxTaskListPerUser(userId);

                        if (!validation.IsSuccess)
                        {
                            customError = new CustomError(false, validation.Error);
                        }
                        else
                        {
                            // If user found, assign the ID to the request
                            request.AssignedToId = userId;
                        }

                    }
                    else
                    {
                        //User not found, add error to response
                        customError = new CustomError(false, $"AssignedTo '{request.AssignedTo}' not found");
                    }
                }


                if (customError.isSuccess)
                {
                    var newTaskList = new TaskList
                    {
                        Title = request.Title,
                        AssignedToId = request.AssignedToId,
                        DueDate = request.DueDate
                    };

                    foreach (var taskItem in request.createTaskItemRequests)
                    {
                        //TODO testing performance
                        await Task.Delay(1000);

                        var newTaskItem = new TaskItem()
                        {
                            Title = taskItem.Title,
                            Description = taskItem.Description
                        };

                        createTaskListResponseList.Add(new CreateTaskListResponse
                        {
                            RowId = taskItem.RowId,
                            CustomError = new CustomError(true)
                        });

                        newTaskList.TaskItems.Add(newTaskItem);
                    }

                    taskLists.Add(newTaskList);
                }
                else
                {
                    foreach (var taskItem in request.createTaskItemRequests)
                    {
                        createTaskListResponseList.Add(new CreateTaskListResponse
                        {
                            RowId = taskItem.RowId,
                            CustomError = customError
                        });
                    }
                }
            }


            var customResulTemp = await _taskListRepository.AddRangeAsync(taskLists);

            return CustomResult<List<CreateTaskListResponse>>.Success(createTaskListResponseList);
        }
        catch (Exception ex)
        {
            createTaskListResponseList.Add(new CreateTaskListResponse
            {
                RowId = 0,
                CustomError = new CustomError(false, $"An error occurred while creating task lists: {ex.Message}")
            });
            return CustomResult<List<CreateTaskListResponse>>.Success(createTaskListResponseList);
        }
    }


    public async Task<CustomResult> UpdateTaskList(int Id, TaskListFormRequest taskListFormRequest)
    {
        if (taskListFormRequest.AssignedToId > 0)
        {
            var validation = await CheckMaxTaskListPerUser((int)taskListFormRequest.AssignedToId);

            if (!validation.IsSuccess) return validation;
        }

        var currentTaskList = await _taskListRepository.GetByIdAsync(Id);

        currentTaskList.Title = taskListFormRequest.Title;
        currentTaskList.AssignedToId = taskListFormRequest.AssignedToId;
        currentTaskList.DueDate = taskListFormRequest.DueDate;

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

    public async Task<CustomResult<TaskListFormRequest>> GetTaskListFormById(int Id, CancellationToken cancellationToken)
    {
        var taskListFormRequest = await _taskListRepository.GetTaskListFormById(Id, cancellationToken);

        if (taskListFormRequest == null) return CustomResult<TaskListFormRequest>.Failure(new CustomError(false, "Not found"));

        return CustomResult<TaskListFormRequest>.Success(taskListFormRequest);
    }

    public async Task<IEnumerable<TaskListWithItemsDto>> GetTaskListWithItemsByUser(int userId, CancellationToken cancellationToken)
    {
        return await _taskListRepository.GetTaskListWithItemsByUser(userId, cancellationToken);
    }

    public async Task<CustomResult> CompleteTaskList(int Id, CancellationToken cancellationToken)
    {
        return await _taskListRepository.CompleteTaskList(Id, cancellationToken);
    }

    public async Task<CustomResult<TaskListDto>> GetTaskListById(int Id, CancellationToken cancellationToken)
    {
        var taskListDto = await _taskListRepository.GetTaskListById(Id, cancellationToken);

        if (taskListDto == null) return CustomResult<TaskListDto>.Failure(new CustomError(false, "Not found"));

        return CustomResult<TaskListDto>.Success(taskListDto);
    }


}