using Application.Aggregates.TaskPriorityAggregate.Queries;
using Application.Common.Models;

namespace ServiceLayer.TaskPriorities
{
    public interface ITaskPriorityService
    {
        Task<IEnumerable<SelectListItem>> GetSelectList();
    }
}
