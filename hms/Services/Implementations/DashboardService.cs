using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Dashboard service implementation
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<DashboardService> _logger;
    private const string DASHBOARD_CACHE_PREFIX = "dashboard_";
    
    public DashboardService(AppDbContext context, ICacheService cache, ILogger<DashboardService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    public async Task<AdminDashboardDto> GetAdminDashboardAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{DASHBOARD_CACHE_PREFIX}admin_{hostelId}";
            
            // Try to get from cache (5 minutes)
            var cached = await _cache.GetAsync<AdminDashboardDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var today = DateTime.UtcNow.Date;
            
            // Get total counts
            var totalHostels = await _context.Hostels.CountAsync(h => !h.IsDeleted);
            
            var totalStudents = await _context.Students
                .CountAsync(s => !s.IsDeleted);
            
            var totalRooms = await _context.Rooms
                .CountAsync(r => !r.IsDeleted);
            
            var occupiedRooms = await _context.Rooms
                .CountAsync(r => !r.IsDeleted && r.CurrentOccupancy > 0);
            
            var totalComplaints = await _context.Complaints
                .CountAsync(c => !c.IsDeleted);
            
            var pendingComplaints = await _context.Complaints
                .CountAsync(c => !c.IsDeleted && c.Status == "Pending");
            
            var totalFeesPending = await _context.Fees
                .Where(f => !f.IsDeleted && f.Status == "Pending")
                .SumAsync(f => f.Amount);
            
            var totalFeesCollected = await _context.Fees
                .Where(f => !f.IsDeleted && f.Status == "Paid")
                .SumAsync(f => f.Amount);
            
            // Cleaning statistics for today
            var todayCleaningRecords = await _context.CleaningRecords
                .Where(c => c.Date.Date == today)
                .Include(c => c.Room)
                .ToListAsync();
            
            var roomsWithRecords = todayCleaningRecords.Select(c => c.RoomId).Distinct().Count();
            var cleanedToday = todayCleaningRecords.Count(c => c.Status == "Cleaned");
            var totalRoomsForCleaning = await _context.Rooms.CountAsync(r => !r.IsDeleted);
            var pendingCleaningRooms = totalRoomsForCleaning - cleanedToday;
            
            var cleaningPercentageToday = totalRoomsForCleaning > 0 
                ? (cleanedToday / (double)totalRoomsForCleaning) * 100 
                : 0;
            
            // Get hostel-wise statistics
            var hostels = await _context.Hostels
                .Where(h => !h.IsDeleted)
                .ToListAsync();
            
            var hostelStats = new List<HostelStatsDto>();
            
            foreach (var hostel in hostels)
            {
                var stats = new HostelStatsDto
                {
                    HostelId = hostel.HostelId,
                    HostelName = hostel.Name,
                    TotalRooms = await _context.Rooms.CountAsync(r => r.HostelId == hostel.HostelId && !r.IsDeleted),
                    OccupiedRooms = await _context.Rooms.CountAsync(r => r.HostelId == hostel.HostelId && !r.IsDeleted && r.CurrentOccupancy > 0),
                    TotalStudents = await _context.Students.CountAsync(s => s.HostelId == hostel.HostelId && !s.IsDeleted),
                    TotalComplaints = await _context.Complaints.CountAsync(c => c.HostelId == hostel.HostelId && !c.IsDeleted),
                    PendingComplaints = await _context.Complaints.CountAsync(c => c.HostelId == hostel.HostelId && c.Status == "Pending" && !c.IsDeleted),
                    TotalFeesPending = await _context.Fees
                        .Where(f => f.HostelId == hostel.HostelId && f.Status == "Pending" && !f.IsDeleted)
                        .SumAsync(f => f.Amount)
                };
                
                hostelStats.Add(stats);
            }
            
            var result = new AdminDashboardDto
            {
                TotalHostels = totalHostels,
                TotalStudents = totalStudents,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                TotalComplaints = totalComplaints,
                PendingComplaints = pendingComplaints,
                TotalFeesPending = totalFeesPending,
                TotalFeesCollected = totalFeesCollected,
                PendingCleaningRooms = pendingCleaningRooms,
                CleanedRoomsToday = cleanedToday,
                CleaningPercentageToday = cleaningPercentageToday,
                HostelStats = hostelStats
            };
            
            // Cache for 5 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting admin dashboard: {ex.Message}");
            return new AdminDashboardDto();
        }
    }
    
    /// <summary>
    /// Get quick statistics for a hostel
    /// </summary>
    public async Task<HostelStatsDto?> GetHostelQuickStatsAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{DASHBOARD_CACHE_PREFIX}quick_{hostelId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<HostelStatsDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var hostel = await _context.Hostels
                .FirstOrDefaultAsync(h => h.HostelId == hostelId && !h.IsDeleted);
            
            if (hostel == null)
            {
                return null;
            }
            
            var result = new HostelStatsDto
            {
                HostelId = hostelId,
                HostelName = hostel.Name,
                TotalRooms = await _context.Rooms.CountAsync(r => r.HostelId == hostelId && !r.IsDeleted),
                OccupiedRooms = await _context.Rooms.CountAsync(r => r.HostelId == hostelId && !r.IsDeleted && r.CurrentOccupancy > 0),
                TotalStudents = await _context.Students.CountAsync(s => s.HostelId == hostelId && !s.IsDeleted),
                TotalComplaints = await _context.Complaints.CountAsync(c => c.HostelId == hostelId && !c.IsDeleted),
                PendingComplaints = await _context.Complaints.CountAsync(c => c.HostelId == hostelId && c.Status == "Pending" && !c.IsDeleted),
                TotalFeesPending = await _context.Fees
                    .Where(f => f.HostelId == hostelId && f.Status == "Pending" && !f.IsDeleted)
                    .SumAsync(f => f.Amount)
            };
            
            // Cache for 10 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting hostel quick stats: {ex.Message}");
            return null;
        }
    }
}
