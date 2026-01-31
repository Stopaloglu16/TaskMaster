using Application.Aggregates.TaskPriorityAggregate.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories;

public interface ITaskPriorityRepository : IRepository<TaskPriority, int>
{
    Task<IEnumerable<SelectListItem>> GetSelectList();
}
