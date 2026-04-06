using SmartHostelManagementSystem.Models.DTOs;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for hostel service
/// </summary>
public interface IHostelService
{
    /// <summary>
    /// Create a new hostel
    /// </summary>
    Task<HostelDto?> CreateHostelAsync(CreateHostelRequest request);
    
    /// <summary>
    /// Get all hostels
    /// </summary>
    Task<List<HostelDto>> GetAllHostelsAsync();
    
    /// <summary>
    /// Get hostel by ID
    /// </summary>
    Task<HostelDto?> GetHostelByIdAsync(int hostelId);
    
    /// <summary>
    /// Get hostel statistics
    /// </summary>
    Task<HostelStatsDto?> GetHostelStatsAsync(int hostelId);
    
    /// <summary>
    /// Update hostel
    /// </summary>
    Task<HostelDto?> UpdateHostelAsync(int hostelId, CreateHostelRequest request);
    
    /// <summary>
    /// Delete hostel (soft delete)
    /// </summary>
    Task<bool> DeleteHostelAsync(int hostelId);
}
