using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for distributed caching service
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task ClearAsync();
}
