using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Service interface for hostel-related operations
/// </summary>
public interface IHostelService
{
    /// <summary>
    /// Create a new hostel
    /// </summary>
    /// <param name="request">Hostel creation request</param>
    /// <returns>Created hostel DTO</returns>
    Task<HostelDto> CreateHostelAsync(CreateHostelRequest request);

    /// <summary>
    /// Get hostel by ID
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Hostel DTO if found, null otherwise</returns>
    Task<HostelDto?> GetHostelByIdAsync(int hostelId);

    /// <summary>
    /// Get all hostels
    /// </summary>
    /// <returns>List of all hostels</returns>
    Task<IEnumerable<HostelDto>> GetAllHostelsAsync();

    /// <summary>
    /// Get hostel with detailed statistics
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Detailed hostel information</returns>
    Task<HostelDto?> GetHostelWithStatsAsync(int hostelId);

    /// <summary>
    /// Update hostel information
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated hostel DTO</returns>
    Task<HostelDto?> UpdateHostelAsync(int hostelId, CreateHostelRequest request);

    /// <summary>
    /// Delete hostel (soft delete)
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteHostelAsync(int hostelId);
}
