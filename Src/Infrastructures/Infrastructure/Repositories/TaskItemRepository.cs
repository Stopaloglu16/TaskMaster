using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class TaskItemRepository : EfCoreRepository<TaskItem, int>, ITaskItemRepository
{

    private readonly ApplicationDbContext _dbContext;

    public TaskItemRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

}
