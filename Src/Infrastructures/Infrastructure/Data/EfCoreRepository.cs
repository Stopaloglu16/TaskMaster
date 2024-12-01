using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class EfCoreRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    private readonly ApplicationDbContext context;
    public EfCoreRepository(ApplicationDbContext context)
    {
        this.context = context;
    }


    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await context.Set<TEntity>().ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllActiveAsync(bool IsActive = true)
    {
        return await context.Set<TEntity>().Where(q => q.IsDeleted == 0).ToListAsync();
    }

    public async Task<TEntity> GetByIdAsync(TKey id)
    {
        return await context.Set<TEntity>().FindAsync(id);
    }


    public async Task<List<TEntity>> ListAllAsync()
    {
        return await context.Set<TEntity>().ToListAsync();
    }


    public async Task<TEntity> AddAsync(TEntity entity)
    {
        context.Set<TEntity>().Add(entity);
        await context.SaveChangesAsync(new CancellationToken());
        return entity;
    }

    public async Task<CustomResult> AddRangeAsync(IEnumerable<TEntity> entity)
    {
        context.Set<TEntity>().AddRange(entity);
        await context.SaveChangesAsync(new CancellationToken());
        return CustomResult.Success();
    }

    public async Task<CustomResult> UpdateAsync(TEntity entity)
    {
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync(new CancellationToken());
        return CustomResult.Success();
    }

    public async Task<CustomResult> DeleteAsync(TKey id)
    {
        var entity = await context.Set<TEntity>().FindAsync(id);
        if (entity == null)
        {
            return CustomResult.Failure("Not found to delete");
        }

        context.Set<TEntity>().Remove(entity);
        await context.SaveChangesAsync(new CancellationToken());

        return CustomResult.Success();
    }

}
