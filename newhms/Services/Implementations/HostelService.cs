using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Implementation of IHostelService for hostel management operations
/// </summary>
public class HostelService : IHostelService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private const string HOSTEL_CACHE_PREFIX = "hostel_";

    public HostelService(ApplicationDbContext context, IMapper mapper, ICacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Create a new hostel with validation
    /// </summary>
    public async Task<HostelDto> CreateHostelAsync(CreateHostelRequest request)
    {
        // Business Rule: Prevent duplicate hostel names
        var existingHostel = await _context.Hostels
            .FirstOrDefaultAsync(h => h.Name == request.Name);

        if (existingHostel != null)
        {
            throw new InvalidOperationException($"Hostel with name '{request.Name}' already exists.");
        }

        var hostel = new Hostel
        {
            Name = request.Name,
            Location = request.Location,
            Description = request.Description,
            PhoneNumber = request.PhoneNumber,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Hostels.Add(hostel);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"{HOSTEL_CACHE_PREFIX}all");

        return MapToHostelDto(hostel);
    }

    /// <summary>
    /// Get hostel by ID with cached results
    /// </summary>
    public async Task<HostelDto?> GetHostelByIdAsync(int hostelId)
    {
        // Try to get from cache first
        var cachedData = await _cacheService.GetAsync<HostelDto>($"{HOSTEL_CACHE_PREFIX}{hostelId}");
        if (cachedData != null)
            return cachedData;

        var hostel = await _context.Hostels.FirstOrDefaultAsync(h => h.HostelId == hostelId);
        
        if (hostel == null)
            return null;

        var dto = MapToHostelDto(hostel);

        // Cache for 1 hour
        await _cacheService.SetAsync($"{HOSTEL_CACHE_PREFIX}{hostelId}", dto, TimeSpan.FromHours(1));

        return dto;
    }

    /// <summary>
    /// Get all hostels with caching
    /// </summary>
    public async Task<IEnumerable<HostelDto>> GetAllHostelsAsync()
    {
        // Try cached data first
        var cachedData = await _cacheService.GetAsync<IEnumerable<HostelDto>>($"{HOSTEL_CACHE_PREFIX}all");
        if (cachedData != null)
            return cachedData;

        var hostels = await _context.Hostels
            .Where(h => !h.IsDeleted)
            .ToListAsync();

        var dtos = hostels.Select(MapToHostelDto).ToList();

        // Cache for 30 minutes
        await _cacheService.SetAsync($"{HOSTEL_CACHE_PREFIX}all", dtos, TimeSpan.FromMinutes(30));

        return dtos;
    }

    /// <summary>
    /// Get hostel with detailed statistics including room and student counts
    /// </summary>
    public async Task<HostelDto?> GetHostelWithStatsAsync(int hostelId)
    {
        var hostel = await _context.Hostels
            .Include(h => h.Rooms)
            .Include(h => h.Students)
            .FirstOrDefaultAsync(h => h.HostelId == hostelId && !h.IsDeleted);

        if (hostel == null)
            return null;

        var dto = MapToHostelDto(hostel);

        // Calculate additional statistics
        var occupiedRooms = hostel.Rooms.Count(r => r.CurrentOccupancy > 0);
        var totalStudents = hostel.Students.Count(s => !s.IsDeleted);

        dto.OccupiedRooms = occupiedRooms;
        dto.AvailableRooms = hostel.Rooms.Count - occupiedRooms;
        dto.TotalStudents = totalStudents;
        dto.TotalRooms = hostel.Rooms.Count;

        return dto;
    }

    /// <summary>
    /// Update hostel information
    /// </summary>
    public async Task<HostelDto?> UpdateHostelAsync(int hostelId, CreateHostelRequest request)
    {
        var hostel = await _context.Hostels.FirstOrDefaultAsync(h => h.HostelId == hostelId);
        
        if (hostel == null)
            return null;

        // Business Rule: Check if new name is not taken
        if (hostel.Name != request.Name)
        {
            var duplicate = await _context.Hostels
                .FirstOrDefaultAsync(h => h.Name == request.Name && h.HostelId != hostelId);

            if (duplicate != null)
                throw new InvalidOperationException($"Hostel name '{request.Name}' is already taken.");
        }

        hostel.Name = request.Name;
        hostel.Location = request.Location;
        hostel.Description = request.Description;
        hostel.PhoneNumber = request.PhoneNumber;
        hostel.UpdatedAt = DateTime.UtcNow;

        _context.Hostels.Update(hostel);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"{HOSTEL_CACHE_PREFIX}{hostelId}");
        await _cacheService.RemoveAsync($"{HOSTEL_CACHE_PREFIX}all");

        return MapToHostelDto(hostel);
    }

    /// <summary>
    /// Delete hostel (soft delete)
    /// </summary>
    public async Task<bool> DeleteHostelAsync(int hostelId)
    {
        var hostel = await _context.Hostels.FirstOrDefaultAsync(h => h.HostelId == hostelId);
        
        if (hostel == null)
            return false;

        hostel.IsDeleted = true;
        hostel.UpdatedAt = DateTime.UtcNow;

        _context.Hostels.Update(hostel);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"{HOSTEL_CACHE_PREFIX}{hostelId}");
        await _cacheService.RemoveAsync($"{HOSTEL_CACHE_PREFIX}all");

        return true;
    }

    /// <summary>
    /// Helper method to map Hostel entity to DTO
    /// </summary>
    private static HostelDto MapToHostelDto(Hostel hostel)
    {
        return new HostelDto
        {
            HostelId = hostel.HostelId,
            Name = hostel.Name,
            Location = hostel.Location,
            Description = hostel.Description,
            PhoneNumber = hostel.PhoneNumber,
            TotalRooms = hostel.Rooms?.Count ?? 0,
            OccupiedRooms = hostel.Rooms?.Count(r => r.CurrentOccupancy > 0) ?? 0,
            AvailableRooms = hostel.Rooms?.Count(r => r.CurrentOccupancy == 0) ?? 0,
            TotalStudents = hostel.Students?.Count(s => !s.IsDeleted) ?? 0,
            CreatedAt = hostel.CreatedAt,
            UpdatedAt = hostel.UpdatedAt
        };
    }
}
