using SmartHostelManagementSystem.Models.DTOs;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for dashboard service
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    Task<AdminDashboardDto> GetAdminDashboardAsync(int hostelId);
    
    /// <summary>
    /// Get quick statistics for a hostel
    /// </summary>
    Task<HostelStatsDto?> GetHostelQuickStatsAsync(int hostelId);
}
