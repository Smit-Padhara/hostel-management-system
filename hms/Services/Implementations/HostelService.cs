using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Hostel service implementation
/// </summary>
public class HostelService : IHostelService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<HostelService> _logger;
    private const string HOSTEL_CACHE_PREFIX = "hostel_";
    
    public HostelService(AppDbContext context, ICacheService cache, ILogger<HostelService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Create a new hostel
    /// </summary>
    public async Task<HostelDto?> CreateHostelAsync(CreateHostelRequest request)
    {
        try
        {
            var hostel = new Hostel
            {
                Name = request.Name,
                Location = request.Location,
                Description = request.Description,
                Capacity = request.Capacity,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Hostels.Add(hostel);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Hostel created: {hostel.HostelId} - {hostel.Name}");
            
            return MapToDto(hostel);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating hostel: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get all hostels
    /// </summary>
    public async Task<List<HostelDto>> GetAllHostelsAsync()
    {
        try
        {
            // Try to get from cache
            var cached = await _cache.GetAsync<List<HostelDto>>("all_hostels");
            if (cached != null)
            {
                return cached;
            }
            
            var hostels = await _context.Hostels
                .Where(h => !h.IsDeleted)
                .ToListAsync();
            
            var result = hostels.Select(MapToDto).ToList();
            
            // Cache for 30 minutes
            await _cache.SetAsync("all_hostels", result, TimeSpan.FromMinutes(30));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all hostels: {ex.Message}");
            return new List<HostelDto>();
        }
    }
    
    /// <summary>
    /// Get hostel by ID
    /// </summary>
    public async Task<HostelDto?> GetHostelByIdAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{HOSTEL_CACHE_PREFIX}{hostelId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<HostelDto>(cacheKey);
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
            
            var result = MapToDto(hostel);
            
            // Cache for 1 hour
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting hostel {hostelId}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get hostel statistics
    /// </summary>
    public async Task<HostelStatsDto?> GetHostelStatsAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{HOSTEL_CACHE_PREFIX}{hostelId}_stats";
            
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
            
            var totalRooms = await _context.Rooms
                .CountAsync(r => r.HostelId == hostelId && !r.IsDeleted);
            
            var occupiedRooms = await _context.Rooms
                .CountAsync(r => r.HostelId == hostelId && !r.IsDeleted && r.CurrentOccupancy > 0);
            
            var totalStudents = await _context.Students
                .CountAsync(s => s.HostelId == hostelId && !s.IsDeleted);
            
            var totalComplaints = await _context.Complaints
                .CountAsync(c => c.HostelId == hostelId && !c.IsDeleted);
            
            var pendingComplaints = await _context.Complaints
                .CountAsync(c => c.HostelId == hostelId && c.Status == "Pending" && !c.IsDeleted);
            
            var totalFeesPending = await _context.Fees
                .Where(f => f.HostelId == hostelId && f.Status == "Pending" && !f.IsDeleted)
                .SumAsync(f => f.Amount);
            
            var result = new HostelStatsDto
            {
                HostelId = hostelId,
                HostelName = hostel.Name,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                TotalStudents = totalStudents,
                TotalComplaints = totalComplaints,
                PendingComplaints = pendingComplaints,
                TotalFeesPending = totalFeesPending
            };
            
            // Cache for 5 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting hostel stats {hostelId}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Update hostel
    /// </summary>
    public async Task<HostelDto?> UpdateHostelAsync(int hostelId, CreateHostelRequest request)
    {
        try
        {
            var hostel = await _context.Hostels.FindAsync(hostelId);
            if (hostel == null || hostel.IsDeleted)
            {
                return null;
            }
            
            hostel.Name = request.Name;
            hostel.Location = request.Location;
            hostel.Description = request.Description;
            hostel.Capacity = request.Capacity;
            hostel.UpdatedAt = DateTime.UtcNow;
            
            _context.Hostels.Update(hostel);
            await _context.SaveChangesAsync();
            
            // Clear cache
            await _cache.RemoveAsync($"{HOSTEL_CACHE_PREFIX}{hostelId}");
            await _cache.RemoveAsync("all_hostels");
            
            _logger.LogInformation($"Hostel updated: {hostelId}");
            
            return MapToDto(hostel);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating hostel {hostelId}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Delete hostel (soft delete)
    /// </summary>
    public async Task<bool> DeleteHostelAsync(int hostelId)
    {
        try
        {
            var hostel = await _context.Hostels.FindAsync(hostelId);
            if (hostel == null || hostel.IsDeleted)
            {
                return false;
            }
            
            hostel.IsDeleted = true;
            hostel.UpdatedAt = DateTime.UtcNow;
            
            _context.Hostels.Update(hostel);
            await _context.SaveChangesAsync();
            
            // Clear cache
            await _cache.RemoveAsync($"{HOSTEL_CACHE_PREFIX}{hostelId}");
            await _cache.RemoveAsync("all_hostels");
            
            _logger.LogInformation($"Hostel deleted: {hostelId}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting hostel {hostelId}: {ex.Message}");
            return false;
        }
    }
    
    private static HostelDto MapToDto(Hostel hostel)
    {
        return new HostelDto
        {
            HostelId = hostel.HostelId,
            Name = hostel.Name,
            Location = hostel.Location,
            Description = hostel.Description,
            Capacity = hostel.Capacity,
            TotalRooms = hostel.Rooms?.Count(r => !r.IsDeleted) ?? 0,
            TotalStudents = hostel.Students?.Count(s => !s.IsDeleted) ?? 0,
            CreatedAt = hostel.CreatedAt
        };
    }
}
