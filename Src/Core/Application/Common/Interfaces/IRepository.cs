using Application.Common.Models;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetAllActiveAsync(bool IsActive = true);
    Task<TEntity> GetByIdAsync(TKey id);
    Task<CustomResult> AddAsync(TEntity entity);
    Task<CustomResult> AddRangeAsync(IEnumerable<TEntity> entity);
    Task<CustomResult> UpdateAsync(TEntity entity);
    Task<CustomResult> DeleteAsync(TKey id);
}