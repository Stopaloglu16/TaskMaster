using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Repositories;

public interface ITaskItemRepository : IRepository<TaskItem, int>
{

}
