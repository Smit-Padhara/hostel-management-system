using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Implementation of IRoomService for room management with capacity control
/// </summary>
public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private const string ROOM_CACHE_PREFIX = "room_";
    private const string AVAILABLE_ROOMS_CACHE_PREFIX = "available_rooms_";

    public RoomService(ApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Create a new room with validation
    /// </summary>
    public async Task<RoomDto> CreateRoomAsync(CreateRoomRequest request)
    {
        // Validate hostel exists
        var hostelsExists = await _context.Hostels
            .AnyAsync(h => h.HostelId == request.HostelId && !h.IsDeleted);

        if (!hostelsExists)
            throw new InvalidOperationException($"Hostel with ID {request.HostelId} does not exist.");

        // Business Rule: Prevent duplicate room numbers in same hostel
        var existingRoom = await _context.Rooms
            .FirstOrDefaultAsync(r => 
                r.HostelId == request.HostelId && 
                r.RoomNumber == request.RoomNumber &&
                !r.IsDeleted);

        if (existingRoom != null)
            throw new InvalidOperationException(
                $"Room number '{request.RoomNumber}' already exists in this hostel.");

        var room = new Room
        {
            HostelId = request.HostelId,
            RoomNumber = request.RoomNumber,
            Capacity = request.Capacity,
            CurrentOccupancy = 0,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"{AVAILABLE_ROOMS_CACHE_PREFIX}{request.HostelId}");

        return MapToRoomDto(room);
    }

    /// <summary>
    /// Get room by ID with caching
    /// </summary>
    public async Task<RoomDto?> GetRoomByIdAsync(int roomId)
    {
        // Try cache first
        var cachedData = await _cacheService.GetAsync<RoomDto>($"{ROOM_CACHE_PREFIX}{roomId}");
        if (cachedData != null)
            return cachedData;

        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId && !r.IsDeleted);
        
        if (room == null)
            return null;

        var dto = MapToRoomDto(room);
        
        // Cache for 30 minutes
        await _cacheService.SetAsync($"{ROOM_CACHE_PREFIX}{roomId}", dto, TimeSpan.FromMinutes(30));

        return dto;
    }

    /// <summary>
    /// Get all rooms in a hostel
    /// </summary>
    public async Task<IEnumerable<RoomDto>> GetRoomsByHostelAsync(int hostelId)
    {
        var rooms = await _context.Rooms
            .Where(r => r.HostelId == hostelId && !r.IsDeleted)
            .ToListAsync();

        return rooms.Select(MapToRoomDto).ToList();
    }

    /// <summary>
    /// Get available rooms (rooms with free capacity) - Core business requirement
    /// </summary>
    public async Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(int hostelId)
    {
        // Try cache first
        var cachedData = await _cacheService.GetAsync<IEnumerable<RoomDto>>(
            $"{AVAILABLE_ROOMS_CACHE_PREFIX}{hostelId}");
        if (cachedData != null)
            return cachedData;

        var availableRooms = await _context.Rooms
            .Where(r => 
                r.HostelId == hostelId && 
                !r.IsDeleted &&
                r.CurrentOccupancy < r.Capacity) // Business Rule: Only rooms with capacity
            .ToListAsync();

        var dtos = availableRooms.Select(MapToRoomDto).ToList();

        // Cache for 15 minutes
        await _cacheService.SetAsync(
            $"{AVAILABLE_ROOMS_CACHE_PREFIX}{hostelId}", 
            dtos, 
            TimeSpan.FromMinutes(15));

        return dtos;
    }

    /// <summary>
    /// Check room capacity - returns available seats
    /// </summary>
    public async Task<int> CheckRoomCapacityAsync(int roomId)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId && !r.IsDeleted);
        
        if (room == null)
            throw new InvalidOperationException($"Room with ID {roomId} not found.");

        // Available seats = Capacity - Current Occupancy
        return room.Capacity - room.CurrentOccupancy;
    }

    /// <summary>
    /// Check if a room is at full capacity
    /// </summary>
    public async Task<bool> IsRoomFullAsync(int roomId)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId && !r.IsDeleted);
        
        if (room == null)
            throw new InvalidOperationException($"Room with ID {roomId} not found.");

        // Business Rule: Prevention of over-allocation
        return room.CurrentOccupancy >= room.Capacity;
    }

    /// <summary>
    /// Increment occupancy when a student is assigned
    /// </summary>
    public async Task<int> IncrementOccupancyAsync(int roomId)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId && !r.IsDeleted);
        
        if (room == null)
            throw new InvalidOperationException($"Room with ID {roomId} not found.");

        // Business Rule: Prevent over-allocation
        if (room.CurrentOccupancy >= room.Capacity)
            throw new InvalidOperationException(
                $"Room {room.RoomNumber} has reached maximum capacity ({room.Capacity}).");

        room.CurrentOccupancy++;
        room.UpdatedAt = DateTime.UtcNow;

        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await InvalidateRoomCache(room.HostelId, roomId);

        return room.CurrentOccupancy;
    }

    /// <summary>
    /// Decrement occupancy when a student is unassigned
    /// </summary>
    public async Task<int> DecrementOccupancyAsync(int roomId)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId && !r.IsDeleted);
        
        if (room == null)
            throw new InvalidOperationException($"Room with ID {roomId} not found.");

        if (room.CurrentOccupancy > 0)
        {
            room.CurrentOccupancy--;
            room.UpdatedAt = DateTime.UtcNow;

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await InvalidateRoomCache(room.HostelId, roomId);
        }

        return room.CurrentOccupancy;
    }

    /// <summary>
    /// Update room information
    /// </summary>
    public async Task<RoomDto?> UpdateRoomAsync(int roomId, CreateRoomRequest request)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId && !r.IsDeleted);
        
        if (room == null)
            return null;

        // Business Rule: Prevent room number duplicate in same hostel
        if (room.RoomNumber != request.RoomNumber)
        {
            var duplicate = await _context.Rooms
                .FirstOrDefaultAsync(r => 
                    r.HostelId == request.HostelId && 
                    r.RoomNumber == request.RoomNumber &&
                    r.RoomId != roomId &&
                    !r.IsDeleted);

            if (duplicate != null)
                throw new InvalidOperationException(
                    $"Room number '{request.RoomNumber}' already exists in this hostel.");
        }

        // Business Rule: Cannot reduce capacity below current occupancy
        if (request.Capacity < room.CurrentOccupancy)
            throw new InvalidOperationException(
                $"Cannot reduce capacity to {request.Capacity} as room currently has {room.CurrentOccupancy} students.");

        room.RoomNumber = request.RoomNumber;
        room.Capacity = request.Capacity;
        room.UpdatedAt = DateTime.UtcNow;

        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await InvalidateRoomCache(room.HostelId, roomId);

        return MapToRoomDto(room);
    }

    /// <summary>
    /// Delete room (soft delete)
    /// </summary>
    public async Task<bool> DeleteRoomAsync(int roomId)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId);
        
        if (room == null)
            return false;

        // Business Rule: Cannot delete room with students assigned
        var studentsInRoom = await _context.Students
            .CountAsync(s => s.RoomId == roomId && !s.IsDeleted);

        if (studentsInRoom > 0)
            throw new InvalidOperationException(
                $"Cannot delete room {room.RoomNumber} as it has {studentsInRoom} students assigned.");

        room.IsDeleted = true;
        room.UpdatedAt = DateTime.UtcNow;

        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await InvalidateRoomCache(room.HostelId, roomId);

        return true;
    }

    /// <summary>
    /// Helper to map Room entity to DTO
    /// </summary>
    private static RoomDto MapToRoomDto(Room room)
    {
        var availableSeats = room.Capacity - room.CurrentOccupancy;
        
        return new RoomDto
        {
            RoomId = room.RoomId,
            HostelId = room.HostelId,
            RoomNumber = room.RoomNumber,
            Capacity = room.Capacity,
            CurrentOccupancy = room.CurrentOccupancy,
            AvailableSeats = availableSeats,
            IsFull = room.CurrentOccupancy >= room.Capacity,
            IsEmpty = room.CurrentOccupancy == 0,
            CreatedAt = room.CreatedAt,
            UpdatedAt = room.UpdatedAt
        };
    }

    /// <summary>
    /// Helper to invalidate room-related cache
    /// </summary>
    private async Task InvalidateRoomCache(int hostelId, int roomId)
    {
        await _cacheService.RemoveAsync($"{ROOM_CACHE_PREFIX}{roomId}");
        await _cacheService.RemoveAsync($"{AVAILABLE_ROOMS_CACHE_PREFIX}{hostelId}");
    }
}
