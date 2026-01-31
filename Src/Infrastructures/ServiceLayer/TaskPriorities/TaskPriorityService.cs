using Application.Aggregates.TaskPriorityAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;

namespace ServiceLayer.TaskPriorities;

public class TaskPriorityService : ITaskPriorityService
{
    private readonly ITaskPriorityRepository _taskPriorityRepository;

    public TaskPriorityService(ITaskPriorityRepository taskPriorityRepository)
    {
        _taskPriorityRepository = taskPriorityRepository;
    }

    public async Task<IEnumerable<SelectListItem>> GetSelectList()
    {
        return await _taskPriorityRepository.GetSelectList();
    }

}
