using SmartHostelManagementSystem.DTOs.Responses;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Service interface for dashboard and analytics
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get dashboard summary for a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Dashboard summary DTO with all metrics</returns>
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(int hostelId);

    /// <summary>
    /// Get recent complaints for dashboard
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <param name="limit">Number of recent complaints to fetch</param>
    /// <returns>List of recent complaints</returns>
    Task<IEnumerable<ComplaintDto>> GetRecentComplaintsAsync(int hostelId, int limit = 5);

    /// <summary>
    /// Get critical alerts for dashboard
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of critical alerts (overdue fees, pending complaints, etc.)</returns>
    Task<Dictionary<string, object>> GetCriticalAlertsAsync(int hostelId);

    /// <summary>
    /// Get hostel utilization metrics
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Utilization metrics</returns>
    Task<Dictionary<string, object>> GetUtilizationMetricsAsync(int hostelId);

    /// <summary>
    /// Get complaint resolution metrics
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Complaint metrics</returns>
    Task<Dictionary<string, object>> GetComplaintMetricsAsync(int hostelId);

    /// <summary>
    /// Get fee collection metrics
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Fee metrics</returns>
    Task<Dictionary<string, object>> GetFeeMetricsAsync(int hostelId);

    /// <summary>
    /// Cache dashboard data in Redis for faster retrieval
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Task completion</returns>
    Task CacheDashboardDataAsync(int hostelId);

    /// <summary>
    /// Invalidate dashboard cache
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Task completion</returns>
    Task InvalidateDashboardCacheAsync(int hostelId);
}
