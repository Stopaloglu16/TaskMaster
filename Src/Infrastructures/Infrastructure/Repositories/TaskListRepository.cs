using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class TaskListRepository : EfCoreRepository<TaskList, int>, ITaskListRepository
{

    private readonly ApplicationDbContext _dbContext;

    public TaskListRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }


}
