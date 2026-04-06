using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Room service implementation
/// </summary>
public class RoomService : IRoomService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<RoomService> _logger;
    private const string ROOM_CACHE_PREFIX = "room_";
    
    public RoomService(AppDbContext context, ICacheService cache, ILogger<RoomService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Create a new room
    /// </summary>
    public async Task<RoomDto?> CreateRoomAsync(CreateRoomRequest request)
    {
        try
        {
            // Verify hostel exists
            var hostel = await _context.Hostels.FindAsync(request.HostelId);
            if (hostel == null || hostel.IsDeleted)
            {
                return null;
            }
            
            // Check room number uniqueness within hostel
            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == request.RoomNumber && r.HostelId == request.HostelId && !r.IsDeleted))
            {
                return null;
            }
            
            var room = new Room
            {
                RoomNumber = request.RoomNumber,
                Capacity = request.Capacity,
                CurrentOccupancy = 0,
                HostelId = request.HostelId,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            
            // Clear hostel cache
            await InvalidateHostelCache(request.HostelId);
            
            _logger.LogInformation($"Room created: {room.RoomId} - {room.RoomNumber}");
            
            return MapToDto(room);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating room: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get all rooms for a hostel
    /// </summary>
    public async Task<List<RoomDto>> GetAllRoomsAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{ROOM_CACHE_PREFIX}all_{hostelId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<List<RoomDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var rooms = await _context.Rooms
                .Where(r => r.HostelId == hostelId && !r.IsDeleted)
                .ToListAsync();
            
            var result = rooms.Select(MapToDto).ToList();
            
            // Cache for 1 hour
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting rooms for hostel {hostelId}: {ex.Message}");
            return new List<RoomDto>();
        }
    }
    
    /// <summary>
    /// Get room by ID
    /// </summary>
    public async Task<RoomDto?> GetRoomByIdAsync(int roomId, int hostelId)
    {
        try
        {
            var cacheKey = $"{ROOM_CACHE_PREFIX}{roomId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<RoomDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.HostelId == hostelId && !r.IsDeleted);
            
            if (room == null)
            {
                return null;
            }
            
            var result = MapToDto(room);
            
            // Cache for 1 hour
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting room {roomId}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get available rooms
    /// </summary>
    public async Task<List<RoomAvailabilityDto>> GetAvailableRoomsAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{ROOM_CACHE_PREFIX}available_{hostelId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<List<RoomAvailabilityDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var rooms = await _context.Rooms
                .Where(r => r.HostelId == hostelId && !r.IsDeleted && r.CurrentOccupancy < r.Capacity)
                .ToListAsync();
            
            var result = rooms.Select(r => new RoomAvailabilityDto
            {
                RoomId = r.RoomId,
                RoomNumber = r.RoomNumber,
                AvailableSpots = r.Capacity - r.CurrentOccupancy
            }).ToList();
            
            // Cache for 30 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting available rooms for hostel {hostelId}: {ex.Message}");
            return new List<RoomAvailabilityDto>();
        }
    }
    
    /// <summary>
    /// Update room
    /// </summary>
    public async Task<RoomDto?> UpdateRoomAsync(int roomId, int hostelId, CreateRoomRequest request)
    {
        try
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.HostelId == hostelId && !r.IsDeleted);
            
            if (room == null)
            {
                return null;
            }
            
            room.RoomNumber = request.RoomNumber;
            room.Capacity = request.Capacity;
            room.UpdatedAt = DateTime.UtcNow;
            
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            
            // Clear cache
            await InvalidateRoomCache(roomId, hostelId);
            
            _logger.LogInformation($"Room updated: {roomId}");
            
            return MapToDto(room);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating room {roomId}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Delete room (soft delete)
    /// </summary>
    public async Task<bool> DeleteRoomAsync(int roomId, int hostelId)
    {
        try
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.HostelId == hostelId && !r.IsDeleted);
            
            if (room == null)
            {
                return false;
            }
            
            room.IsDeleted = true;
            room.UpdatedAt = DateTime.UtcNow;
            
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            
            // Clear cache
            await InvalidateRoomCache(roomId, hostelId);
            
            _logger.LogInformation($"Room deleted: {roomId}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting room {roomId}: {ex.Message}");
            return false;
        }
    }
    
    private static RoomDto MapToDto(Room room)
    {
        return new RoomDto
        {
            RoomId = room.RoomId,
            RoomNumber = room.RoomNumber,
            Capacity = room.Capacity,
            CurrentOccupancy = room.CurrentOccupancy,
            HostelId = room.HostelId,
            CreatedAt = room.CreatedAt
        };
    }
    
    private async Task InvalidateRoomCache(int roomId, int hostelId)
    {
        await _cache.RemoveAsync($"{ROOM_CACHE_PREFIX}{roomId}");
        await _cache.RemoveAsync($"{ROOM_CACHE_PREFIX}all_{hostelId}");
        await _cache.RemoveAsync($"{ROOM_CACHE_PREFIX}available_{hostelId}");
    }
    
    private async Task InvalidateHostelCache(int hostelId)
    {
        await _cache.RemoveAsync($"hostel_{hostelId}");
        await _cache.RemoveAsync($"hostel_{hostelId}_stats");
    }
}
