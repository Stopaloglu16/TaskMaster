using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Repositories;


public interface ITaskListRepository : IRepository<TaskList, int>
{

}
