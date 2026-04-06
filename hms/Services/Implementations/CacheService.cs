using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Redis-based caching service implementation
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    
    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Get value from cache
    /// </summary>
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _cache.GetStringAsync(key);
            if (value == null)
            {
                return default;
            }
            
            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting cache for key {key}: {ex.Message}");
            return default;
        }
    }
    
    /// <summary>
    /// Set value in cache with optional expiration
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions();
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                // Default expiration of 1 hour
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            }
            
            await _cache.SetStringAsync(key, serialized, options);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error setting cache for key {key}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Remove value from cache
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error removing cache for key {key}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Clear all cache (not typically supported by Redis, so we log a message)
    /// </summary>
    public async Task ClearAsync()
    {
        try
        {
            _logger.LogWarning("Clear all cache requested. Note: This operation is not fully supported with Redis.");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error clearing cache: {ex.Message}");
        }
    }
}
