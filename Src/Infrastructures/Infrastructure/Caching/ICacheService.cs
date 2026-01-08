using Application.Common.Models;

namespace Infrastructure.Caching
{
    public interface ICacheService
    {
        Task<CustomResult<T?>> GetAsync<T>(string key);
        Task<CustomResult> SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null);
        Task<CustomResult> RemoveAsync<T>(string key);
    }
}
