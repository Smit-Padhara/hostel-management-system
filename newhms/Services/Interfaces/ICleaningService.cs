using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Service interface for cleaning management and tracking
/// </summary>
public interface ICleaningService
{
    /// <summary>
    /// Create a cleaning record for a room
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <param name="date">Date for which cleaning is scheduled</param>
    /// <returns>Created cleaning record DTO</returns>
    Task<CleaningRecordDto> CreateCleaningRecordAsync(int roomId, DateTime date);

    /// <summary>
    /// Get cleaning record by ID
    /// </summary>
    /// <param name="recordId">ID of the cleaning record</param>
    /// <returns>Cleaning record DTO if found</returns>
    Task<CleaningRecordDto?> GetCleaningRecordByIdAsync(int recordId);

    /// <summary>
    /// Get all cleaning records for a room
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <returns>List of cleaning records for the room</returns>
    Task<IEnumerable<CleaningRecordDto>> GetCleaningRecordsByRoomAsync(int roomId);

    /// <summary>
    /// Get today's cleaning tasks for a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of cleaning records for today</returns>
    Task<IEnumerable<CleaningRecordDto>> GetTodaysCleaningTasksAsync(int hostelId);

    /// <summary>
    /// Get pending cleaning records (not yet cleaned)
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of pending cleaning records</returns>
    Task<IEnumerable<CleaningRecordDto>> GetPendingCleaningRecordsAsync(int hostelId);

    /// <summary>
    /// Get pending rooms count
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Number of rooms pending cleaning</returns>
    Task<int> GetPendingCleaningCountAsync(int hostelId);

    /// <summary>
    /// Update cleaning record status
    /// </summary>
    /// <param name="recordId">ID of the cleaning record</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated cleaning record DTO</returns>
    Task<CleaningRecordDto?> UpdateCleaningStatusAsync(int recordId, UpdateCleaningStatusRequest request);

    /// <summary>
    /// Mark room as cleaned
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <param name="workerId">ID of the worker who cleaned (optional)</param>
    /// <param name="remarks">Cleaning remarks (optional)</param>
    /// <returns>Updated cleaning record DTO</returns>
    Task<CleaningRecordDto?> MarkRoomAsCleanedAsync(int roomId, int? workerId = null, string? remarks = null);

    /// <summary>
    /// Get cleaning history for a room
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <param name="days">Number of days to look back</param>
    /// <returns>List of cleaning records for the specified period</returns>
    Task<IEnumerable<CleaningRecordDto>> GetCleaningHistoryAsync(int roomId, int days = 30);

    /// <summary>
    /// Get cleaning report for a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <param name="date">Date for the report</param>
    /// <returns>List of cleaning records for the specified date</returns>
    Task<IEnumerable<CleaningRecordDto>> GetCleaningReportAsync(int hostelId, DateTime date);

    /// <summary>
    /// Identify rooms that haven't been cleaned recently
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <param name="daysThreshold">Number of days threshold</param>
    /// <returns>List of uncleaned rooms</returns>
    Task<IEnumerable<CleaningRecordDto>> GetUncleanlRoomsAsync(int hostelId, int daysThreshold = 3);

    /// <summary>
    /// Delete cleaning record (soft delete)
    /// </summary>
    /// <param name="recordId">ID of the cleaning record</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteCleaningRecordAsync(int recordId);
}
