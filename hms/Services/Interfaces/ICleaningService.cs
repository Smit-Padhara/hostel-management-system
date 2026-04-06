using SmartHostelManagementSystem.Models.DTOs;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for cleaning service
/// </summary>
public interface ICleaningService
{
    /// <summary>
    /// Mark room as cleaned
    /// </summary>
    Task<CleaningRecordDto?> MarkAsCleanedAsync(int workerId, int hostelId, MarkCleaningRequest request);
    
    /// <summary>
    /// Get cleaning records for today
    /// </summary>
    Task<DailyCleaningReportDto?> GetTodayCleaningReportAsync(int hostelId);
    
    /// <summary>
    /// Get pending cleaning rooms
    /// </summary>
    Task<List<PendingCleaningDto>> GetPendingCleaningAsync(int hostelId);
    
    /// <summary>
    /// Get cleaning history for a room
    /// </summary>
    Task<List<CleaningRecordDto>> GetRoomCleaningHistoryAsync(int roomId, int hostelId, int days = 30);
    
    /// <summary>
    /// Get cleaning records for a date range
    /// </summary>
    Task<List<CleaningRecordDto>> GetCleaningRecordsByDateRangeAsync(int hostelId, DateTime startDate, DateTime endDate);
}
