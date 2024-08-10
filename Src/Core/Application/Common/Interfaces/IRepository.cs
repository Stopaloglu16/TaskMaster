using Domain.Common;

namespace Application.Common.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetAllActiveAsync(bool IsActive = true);
    Task<TEntity> GetByIdAsync(TKey id);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TKey id);
}