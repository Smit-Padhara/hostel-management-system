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
    private const int DefaultCacheMinutes = 60; // Default 1 hour TTL
    
    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Get value from cache by key
    /// </summary>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key);
            
            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }
            
            var value = JsonSerializer.Deserialize<T>(cachedValue);
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
            return null;
        }
    }
    
    /// <summary>
    /// Set value in cache with optional expiration
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions();
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(DefaultCacheMinutes);
            }
            
            await _cache.SetStringAsync(key, json, options);
            _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}",
                key, expiration?.TotalMinutes ?? DefaultCacheMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }
    
    /// <summary>
    /// Remove value from cache by key
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key: {Key}", key);
        }
    }
    
    /// <summary>
    /// Clear all cache (not recommended in production)
    /// </summary>
    public async Task ClearAsync()
    {
        try
        {
            // Note: Redis doesn't have a built-in "clear all" for specific keys
            // In production, consider using Prefix-based keys and managing them explicitly
            _logger.LogWarning("Cache clear requested - consider managing prefixes explicitly");
            
            // This is a placeholder - implement prefix-based clearing in production
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }
    }
}
