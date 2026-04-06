using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Cleaning service implementation
/// </summary>
public class CleaningService : ICleaningService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<CleaningService> _logger;
    private const string CLEANING_CACHE_PREFIX = "cleaning_";
    
    public CleaningService(AppDbContext context, ICacheService cache, ILogger<CleaningService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Mark room as cleaned
    /// </summary>
    public async Task<CleaningRecordDto?> MarkAsCleanedAsync(int workerId, int hostelId, MarkCleaningRequest request)
    {
        try
        {
            // Verify worker exists
            var worker = await _context.Workers
                .Where(w => w.WorkerId == workerId && w.HostelId == hostelId && !w.IsDeleted)
                .Include(w => w.User)
                .FirstOrDefaultAsync();
            
            if (worker == null)
            {
                return null;
            }
            
            // Verify room exists
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == request.RoomId && r.HostelId == hostelId && !r.IsDeleted);
            
            if (room == null)
            {
                return null;
            }
            
            var today = DateTime.UtcNow.Date;
            
            // Check if record already exists for today
            var existingRecord = await _context.CleaningRecords
                .FirstOrDefaultAsync(c => c.RoomId == request.RoomId && 
                                         c.WorkerId == workerId && 
                                         c.Date.Date == today);
            
            CleaningRecord record;
            
            if (existingRecord != null)
            {
                record = existingRecord;
            }
            else
            {
                record = new CleaningRecord
                {
                    RoomId = request.RoomId,
                    WorkerId = workerId,
                    Date = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CleaningRecords.Add(record);
            }
            
            record.Status = request.Status;
            record.Remarks = request.Remarks;
            record.UpdatedAt = DateTime.UtcNow;
            
            if (request.Status == "Cleaned")
            {
                record.CleanedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();
            
            // Clear cache
            await InvalidateCleaningCache(hostelId);
            
            _logger.LogInformation($"Cleaning record created/updated: {record.RecordId}");
            
            return await GetCleaningRecordDtoAsync(record, room, worker);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking cleaning: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get cleaning records for today
    /// </summary>
    public async Task<DailyCleaningReportDto?> GetTodayCleaningReportAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{CLEANING_CACHE_PREFIX}today_{hostelId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<DailyCleaningReportDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var today = DateTime.UtcNow.Date;
            
            var records = await _context.CleaningRecords
                .Where(c => c.Date.Date == today && c.Room!.HostelId == hostelId)
                .Include(c => c.Room)
                .Include(c => c.Worker)
                .ThenInclude(w => w!.User)
                .ToListAsync();
            
            var rooms = await _context.Rooms
                .Where(r => r.HostelId == hostelId && !r.IsDeleted)
                .ToListAsync();
            
            var cleanedRooms = records.Count(r => r.Status == "Cleaned");
            var recordDtos = records.Select(r => new CleaningRecordDto
            {
                RecordId = r.RecordId,
                RoomId = r.RoomId,
                RoomNumber = r.Room?.RoomNumber ?? "Unknown",
                WorkerId = r.WorkerId,
                WorkerName = r.Worker?.User?.Name ?? "Unknown",
                Date = r.Date,
                Status = r.Status,
                Remarks = r.Remarks,
                CleanedAt = r.CleanedAt
            }).ToList();
            
            var result = new DailyCleaningReportDto
            {
                Date = today,
                TotalRooms = rooms.Count,
                CleanedRooms = cleanedRooms,
                PendingRooms = rooms.Count - cleanedRooms,
                Records = recordDtos
            };
            
            // Cache for 30 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting today's cleaning report: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get pending cleaning rooms
    /// </summary>
    public async Task<List<PendingCleaningDto>> GetPendingCleaningAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{CLEANING_CACHE_PREFIX}pending_{hostelId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<List<PendingCleaningDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var today = DateTime.UtcNow.Date;
            
            // Get all rooms in the hostel
            var rooms = await _context.Rooms
                .Where(r => r.HostelId == hostelId && !r.IsDeleted)
                .ToListAsync();
            
            // Get today's cleaning records
            var todayRecords = await _context.CleaningRecords
                .Where(c => c.Date.Date == today && c.Room!.HostelId == hostelId)
                .Include(c => c.Worker)
                .ThenInclude(w => w!.User)
                .ToListAsync();
            
            var result = new List<PendingCleaningDto>();
            
            foreach (var room in rooms)
            {
                var record = todayRecords.FirstOrDefault(r => r.RoomId == room.RoomId);
                
                if (record == null || record.Status == "Pending")
                {
                    result.Add(new PendingCleaningDto
                    {
                        RoomId = room.RoomId,
                        RoomNumber = room.RoomNumber,
                        LastCleanedAt = record?.CleanedAt ?? DateTime.MinValue,
                        AssignedWorkerId = record?.WorkerId,
                        AssignedWorkerName = record?.Worker?.User?.Name
                    });
                }
            }
            
            // Cache for 15 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting pending cleaning rooms: {ex.Message}");
            return new List<PendingCleaningDto>();
        }
    }
    
    /// <summary>
    /// Get cleaning history for a room
    /// </summary>
    public async Task<List<CleaningRecordDto>> GetRoomCleaningHistoryAsync(int roomId, int hostelId, int days = 30)
    {
        try
        {
            var cacheKey = $"{CLEANING_CACHE_PREFIX}history_{roomId}_{days}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<List<CleaningRecordDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var startDate = DateTime.UtcNow.AddDays(-days);
            
            var records = await _context.CleaningRecords
                .Where(c => c.RoomId == roomId && 
                           c.Date >= startDate &&
                           c.Room!.HostelId == hostelId)
                .Include(c => c.Room)
                .Include(c => c.Worker)
                .ThenInclude(w => w!.User)
                .OrderByDescending(c => c.Date)
                .ToListAsync();
            
            var result = records.Select(r => new CleaningRecordDto
            {
                RecordId = r.RecordId,
                RoomId = r.RoomId,
                RoomNumber = r.Room?.RoomNumber ?? "Unknown",
                WorkerId = r.WorkerId,
                WorkerName = r.Worker?.User?.Name ?? "Unknown",
                Date = r.Date,
                Status = r.Status,
                Remarks = r.Remarks,
                CleanedAt = r.CleanedAt
            }).ToList();
            
            // Cache for 1 hour
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting cleaning history: {ex.Message}");
            return new List<CleaningRecordDto>();
        }
    }
    
    /// <summary>
    /// Get cleaning records for a date range
    /// </summary>
    public async Task<List<CleaningRecordDto>> GetCleaningRecordsByDateRangeAsync(int hostelId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await _context.CleaningRecords
                .Where(c => c.Date >= startDate && 
                           c.Date <= endDate &&
                           c.Room!.HostelId == hostelId)
                .Include(c => c.Room)
                .Include(c => c.Worker)
                .ThenInclude(w => w!.User)
                .OrderByDescending(c => c.Date)
                .ToListAsync();
            
            return records.Select(r => new CleaningRecordDto
            {
                RecordId = r.RecordId,
                RoomId = r.RoomId,
                RoomNumber = r.Room?.RoomNumber ?? "Unknown",
                WorkerId = r.WorkerId,
                WorkerName = r.Worker?.User?.Name ?? "Unknown",
                Date = r.Date,
                Status = r.Status,
                Remarks = r.Remarks,
                CleanedAt = r.CleanedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting cleaning records by date range: {ex.Message}");
            return new List<CleaningRecordDto>();
        }
    }
    
    private static async Task<CleaningRecordDto> GetCleaningRecordDtoAsync(CleaningRecord record, Room room, Worker worker)
    {
        return new CleaningRecordDto
        {
            RecordId = record.RecordId,
            RoomId = record.RoomId,
            RoomNumber = room.RoomNumber,
            WorkerId = record.WorkerId,
            WorkerName = worker.User?.Name ?? "Unknown",
            Date = record.Date,
            Status = record.Status,
            Remarks = record.Remarks,
            CleanedAt = record.CleanedAt
        };
    }
    
    private async Task InvalidateCleaningCache(int hostelId)
    {
        await _cache.RemoveAsync($"{CLEANING_CACHE_PREFIX}today_{hostelId}");
        await _cache.RemoveAsync($"{CLEANING_CACHE_PREFIX}pending_{hostelId}");
        await _cache.RemoveAsync($"hostel_{hostelId}_stats");
    }
}
