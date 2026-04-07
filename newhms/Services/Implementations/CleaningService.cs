using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Implementation of ICleaningService for cleaning management (CORE MODULE)
/// Handles daily cleaning tasks, room status tracking, and cleaning reports
/// </summary>
public class CleaningService : ICleaningService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private const string CLEANING_CACHE_PREFIX = "cleaning_";
    private const string PENDING_CLEANING_CACHE_PREFIX = "pending_cleaning_";
    private const string TODAY_CLEANING_CACHE_PREFIX = "today_cleaning_";

    public CleaningService(ApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Create a cleaning record for a room
    /// </summary>
    public async Task<CleaningRecordDto> CreateCleaningRecordAsync(int roomId, DateTime date)
    {
        // Validate: Room exists
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.RoomId == roomId && !r.IsDeleted);

        if (room == null)
            throw new InvalidOperationException($"Room with ID {roomId} not found.");

        // Check for existing record on same date
        var existingRecord = await _context.CleaningRecords
            .FirstOrDefaultAsync(cr => 
                cr.RoomId == roomId && 
                cr.Date.Date == date.Date &&
                !cr.IsDeleted);

        if (existingRecord != null)
            throw new InvalidOperationException(
                $"Cleaning record already exists for room {room.RoomNumber} on {date:yyyy-MM-dd}");

        var record = new CleaningRecord
        {
            RoomId = roomId,
            Date = date,
            Status = "Pending",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.CleaningRecords.Add(record);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await InvalidateCleaningCache(room.HostelId);

        return MapToCleaningRecordDto(record, room);
    }

    /// <summary>
    /// Get cleaning record by ID
    /// </summary>
    public async Task<CleaningRecordDto?> GetCleaningRecordByIdAsync(int recordId)
    {
        // Try cache first
        var cachedData = await _cacheService.GetAsync<CleaningRecordDto>($"{CLEANING_CACHE_PREFIX}{recordId}");
        if (cachedData != null)
            return cachedData;

        var record = await _context.CleaningRecords
            .Include(cr => cr.Room)
            .Include(cr => cr.Worker)
            .FirstOrDefaultAsync(cr => cr.RecordId == recordId && !cr.IsDeleted);

        if (record == null)
            return null;

        var dto = MapToCleaningRecordDto(record, record.Room);
        
        // Cache for 30 minutes
        await _cacheService.SetAsync($"{CLEANING_CACHE_PREFIX}{recordId}", dto, TimeSpan.FromMinutes(30));

        return dto;
    }

    /// <summary>
    /// Get all cleaning records for a specific room
    /// </summary>
    public async Task<IEnumerable<CleaningRecordDto>> GetCleaningRecordsByRoomAsync(int roomId)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId && !r.IsDeleted);
        if (room == null)
            return Enumerable.Empty<CleaningRecordDto>();

        var records = await _context.CleaningRecords
            .Include(cr => cr.Room)
            .Include(cr => cr.Worker)
            .Where(cr => cr.RoomId == roomId && !cr.IsDeleted)
            .OrderByDescending(cr => cr.Date)
            .ToListAsync();

        return records.Select(r => MapToCleaningRecordDto(r, r.Room)).ToList();
    }

    /// <summary>
    /// Get today's cleaning tasks for a hostel (CORE REQUIREMENT)
    /// </summary>
    public async Task<IEnumerable<CleaningRecordDto>> GetTodaysCleaningTasksAsync(int hostelId)
    {
        var today = DateTime.UtcNow.Date;

        // Try cache first
        var cacheKey = $"{TODAY_CLEANING_CACHE_PREFIX}{hostelId}_{today:yyyyMMdd}";
        var cachedData = await _cacheService.GetAsync<IEnumerable<CleaningRecordDto>>(cacheKey);
        if (cachedData != null)
            return cachedData;

        var todaysTasks = await _context.CleaningRecords
            .Include(cr => cr.Room)
            .ThenInclude(r => r!.Hostel)
            .Include(cr => cr.Worker)
            .Where(cr => 
                cr.Room!.HostelId == hostelId &&
                cr.Date.Date == today &&
                !cr.IsDeleted)
            .OrderBy(cr => cr.Status == "Pending" ? 0 : 1) // Pending tasks first
            .ThenBy(cr => cr.Room!.RoomNumber)
            .ToListAsync();

        var dtos = todaysTasks.Select(r => MapToCleaningRecordDto(r, r.Room)).ToList();

        // Cache for 1 hour (updates throughout the day)
        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromHours(1));

        return dtos;
    }

    /// <summary>
    /// Get pending cleaning records (not yet cleaned) - CORE REQUIREMENT
    /// </summary>
    public async Task<IEnumerable<CleaningRecordDto>> GetPendingCleaningRecordsAsync(int hostelId)
    {
        // Try cache first
        var cachedData = await _cacheService.GetAsync<IEnumerable<CleaningRecordDto>>(
            $"{PENDING_CLEANING_CACHE_PREFIX}{hostelId}");
        if (cachedData != null)
            return cachedData;

        var pendingRecords = await _context.CleaningRecords
            .Include(cr => cr.Room)
            .ThenInclude(r => r!.Hostel)
            .Include(cr => cr.Worker)
            .Where(cr => 
                cr.Room!.HostelId == hostelId &&
                cr.Status == "Pending" &&
                !cr.IsDeleted)
            .OrderBy(cr => cr.Date) // Older tasks first
            .ThenBy(cr => cr.Room!.RoomNumber)
            .ToListAsync();

        var dtos = pendingRecords.Select(r => MapToCleaningRecordDto(r, r.Room)).ToList();

        // Cache for 15 minutes
        await _cacheService.SetAsync($"{PENDING_CLEANING_CACHE_PREFIX}{hostelId}", dtos, TimeSpan.FromMinutes(15));

        return dtos;
    }

    /// <summary>
    /// Get count of pending cleaning rooms - CORE REQUIREMENT
    /// </summary>
    public async Task<int> GetPendingCleaningCountAsync(int hostelId)
    {
        return await _context.CleaningRecords
            .Include(cr => cr.Room)
            .CountAsync(cr => 
                cr.Room!.HostelId == hostelId &&
                cr.Status == "Pending" &&
                !cr.IsDeleted);
    }

    /// <summary>
    /// Update cleaning record status
    /// </summary>
    public async Task<CleaningRecordDto?> UpdateCleaningStatusAsync(
        int recordId, 
        UpdateCleaningStatusRequest request)
    {
        var record = await _context.CleaningRecords
            .Include(cr => cr.Room)
            .Include(cr => cr.Worker)
            .FirstOrDefaultAsync(cr => cr.RecordId == recordId && !cr.IsDeleted);

        if (record == null)
            return null;

        record.Status = request.Status;
        record.Remarks = request.Remarks;

        // Business Rule: Update cleaned timestamp when marking as cleaned
        if (request.Status == "Cleaned" && record.Status != "Cleaned")
        {
            record.CleanedAt = DateTime.UtcNow;
        }

        if (request.WorkerId.HasValue)
        {
            record.WorkerId = request.WorkerId;
        }

        record.UpdatedAt = DateTime.UtcNow;

        _context.CleaningRecords.Update(record);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await InvalidateCleaningCache(record.Room?.HostelId ?? 0);

        return MapToCleaningRecordDto(record, record.Room);
    }

    /// <summary>
    /// Mark room as cleaned - Convenience method
    /// </summary>
    public async Task<CleaningRecordDto?> MarkRoomAsCleanedAsync(
        int roomId, 
        int? workerId = null, 
        string? remarks = null)
    {
        var today = DateTime.UtcNow.Date;

        var record = await _context.CleaningRecords
            .Include(cr => cr.Room)
            .FirstOrDefaultAsync(cr => 
                cr.RoomId == roomId &&
                cr.Date.Date == today &&
                !cr.IsDeleted);

        if (record == null)
            return null;

        var updateRequest = new UpdateCleaningStatusRequest
        {
            Status = "Cleaned",
            Remarks = remarks,
            WorkerId = workerId
        };

        return await UpdateCleaningStatusAsync(record.RecordId, updateRequest);
    }

    /// <summary>
    /// Get cleaning history for a room (CORE REQUIREMENT)
    /// </summary>
    public async Task<IEnumerable<CleaningRecordDto>> GetCleaningHistoryAsync(int roomId, int days = 30)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId && !r.IsDeleted);
        if (room == null)
            return Enumerable.Empty<CleaningRecordDto>();

        var startDate = DateTime.UtcNow.AddDays(-days);

        var history = await _context.CleaningRecords
            .Include(cr => cr.Room)
            .Include(cr => cr.Worker)
            .Where(cr => 
                cr.RoomId == roomId &&
                cr.Date >= startDate &&
                !cr.IsDeleted)
            .OrderByDescending(cr => cr.Date)
            .ToListAsync();

        return history.Select(r => MapToCleaningRecordDto(r, r.Room)).ToList();
    }

    /// <summary>
    /// Get cleaning report for a hostel on a specific date (CORE REQUIREMENT)
    /// </summary>
    public async Task<IEnumerable<CleaningRecordDto>> GetCleaningReportAsync(int hostelId, DateTime date)
    {
        var targetDate = date.Date;

        var report = await _context.CleaningRecords
            .Include(cr => cr.Room)
            .ThenInclude(r => r!.Hostel)
            .Include(cr => cr.Worker)
            .Where(cr => 
                cr.Room!.HostelId == hostelId &&
                cr.Date.Date == targetDate &&
                !cr.IsDeleted)
            .OrderBy(cr => cr.Room!.RoomNumber)
            .ToListAsync();

        return report.Select(r => MapToCleaningRecordDto(r, r.Room)).ToList();
    }

    /// <summary>
    /// Identify rooms that haven't been cleaned recently (CORE REQUIREMENT)
    /// </summary>
    public async Task<IEnumerable<CleaningRecordDto>> GetUncleanlRoomsAsync(int hostelId, int daysThreshold = 3)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysThreshold);

        var uncleanlRooms = await _context.CleaningRecords
            .Include(cr => cr.Room)
            .ThenInclude(r => r!.Hostel)
            .Include(cr => cr.Worker)
            .Where(cr => 
                cr.Room!.HostelId == hostelId &&
                (cr.CleanedAt == null || cr.CleanedAt < cutoffDate) &&
                !cr.IsDeleted)
            .GroupBy(cr => cr.RoomId)
            .SelectMany(g => g.OrderByDescending(cr => cr.Date).Take(1))
            .OrderBy(cr => cr.CleanedAt ?? cr.Date)
            .ToListAsync();

        return uncleanlRooms.Select(r => MapToCleaningRecordDto(r, r.Room)).ToList();
    }

    /// <summary>
    /// Delete cleaning record (soft delete)
    /// </summary>
    public async Task<bool> DeleteCleaningRecordAsync(int recordId)
    {
        var record = await _context.CleaningRecords
            .Include(cr => cr.Room)
            .FirstOrDefaultAsync(cr => cr.RecordId == recordId);

        if (record == null)
            return false;

        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;

        _context.CleaningRecords.Update(record);
        await _context.SaveChangesAsync();

        // Invalidate cache
        if (record.Room != null)
            await InvalidateCleaningCache(record.Room.HostelId);

        return true;
    }

    /// <summary>
    /// Helper to map CleaningRecord entity to DTO
    /// </summary>
    private static CleaningRecordDto MapToCleaningRecordDto(CleaningRecord record, Room? room)
    {
        int? daysOverdue = record.Status == "Pending"
            ? (int)(DateTime.UtcNow - record.Date).TotalDays
            : null;

        return new CleaningRecordDto
        {
            RecordId = record.RecordId,
            RoomId = record.RoomId,
            RoomNumber = room?.RoomNumber ?? "N/A",
            WorkerId = record.WorkerId,
            WorkerName = record.Worker?.FullName,
            Date = record.Date,
            Status = record.Status,
            Remarks = record.Remarks,
            CleanedAt = record.CleanedAt,
            DaysOverdue = daysOverdue,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt
        };
    }

    /// <summary>
    /// Helper to invalidate cleaning-related cache
    /// </summary>
    private async Task InvalidateCleaningCache(int hostelId)
    {
        await _cacheService.RemoveAsync($"{PENDING_CLEANING_CACHE_PREFIX}{hostelId}");
        await _cacheService.RemoveAsync($"{TODAY_CLEANING_CACHE_PREFIX}{hostelId}_{DateTime.UtcNow.Date:yyyyMMdd}");
    }
}
