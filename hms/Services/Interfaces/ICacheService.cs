namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for caching service
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key);
    
    /// <summary>
    /// Set value in cache
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    
    /// <summary>
    /// Remove value from cache
    /// </summary>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Clear all cache
    /// </summary>
    Task ClearAsync();
}
