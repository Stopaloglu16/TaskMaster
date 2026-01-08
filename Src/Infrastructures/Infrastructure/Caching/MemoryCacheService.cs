using Application.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Caching;

public class MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
 : ICacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<MemoryCacheService> _logger = logger;

    public Task<CustomResult<T?>> GetAsync<T>(string key)
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out var value))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return Task.FromResult(CustomResult<T?>.Success((T?)value));
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return Task.FromResult(CustomResult<T?>.Success(default(T?)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting value from memory cache. Key: {Key}", key);
            return Task.FromResult(CustomResult<T?>.Failure(new CustomError(false, $"Failed to get cache value: {ex.Message}")));
        }
    }

    public Task<CustomResult> RemoveAsync<T>(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("Cache key removed: {Key}", key);
            return Task.FromResult(CustomResult.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while removing value from memory cache. Key: {Key}", key);
            return Task.FromResult(CustomResult.Failure($"Failed to remove cache value: {ex.Message}"));
        }
    }

    public Task<CustomResult> SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null)
    {
        try
        {
            if (absoluteExpiration.HasValue)
            {
                _memoryCache.Set(key, value, absoluteExpiration.Value);
            }
            else
            {
                _memoryCache.Set(key, value);
            }

            _logger.LogDebug("Cache value set for key: {Key}, Expiration: {Expiration}", key, absoluteExpiration);
            return Task.FromResult(CustomResult.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while setting value to memory cache. Key: {Key}", key);
            return Task.FromResult(CustomResult.Failure($"Failed to set cache value: {ex.Message}"));
        }
    }
}