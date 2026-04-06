using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Student service implementation
/// </summary>
public class StudentService : IStudentService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<StudentService> _logger;
    private const string STUDENT_CACHE_PREFIX = "student_";
    
    public StudentService(AppDbContext context, ICacheService cache, ILogger<StudentService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Create a new student
    /// </summary>
    public async Task<StudentDto?> CreateStudentAsync(CreateStudentRequest request)
    {
        try
        {
            // Verify user exists
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null || user.IsDeleted || user.Role != "Student")
            {
                return null;
            }
            
            // Check if student already exists
            if (await _context.Students.AnyAsync(s => s.UserId == request.UserId && !s.IsDeleted))
            {
                return null;
            }
            
            // Verify hostel exists
            var hostel = await _context.Hostels.FindAsync(request.HostelId);
            if (hostel == null || hostel.IsDeleted)
            {
                return null;
            }
            
            var student = new Student
            {
                UserId = request.UserId,
                HostelId = request.HostelId,
                RoomId = request.RoomId,
                RollNumber = request.RollNumber,
                AdmissionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Student created: {student.StudentId}");
            
            return await GetStudentByIdAsync(student.StudentId, request.HostelId);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating student: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get all students in a hostel
    /// </summary>
    public async Task<List<StudentDto>> GetAllStudentsAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{STUDENT_CACHE_PREFIX}all_{hostelId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<List<StudentDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var students = await _context.Students
                .Where(s => s.HostelId == hostelId && !s.IsDeleted)
                .Include(s => s.User)
                .Include(s => s.Room)
                .ToListAsync();
            
            var result = students.Select(MapToDto).ToList();
            
            // Cache for 30 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting students for hostel {hostelId}: {ex.Message}");
            return new List<StudentDto>();
        }
    }
    
    /// <summary>
    /// Get student by ID
    /// </summary>
    public async Task<StudentDto?> GetStudentByIdAsync(int studentId, int hostelId)
    {
        try
        {
            var cacheKey = $"{STUDENT_CACHE_PREFIX}{studentId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<StudentDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var student = await _context.Students
                .Where(s => s.StudentId == studentId && s.HostelId == hostelId && !s.IsDeleted)
                .Include(s => s.User)
                .Include(s => s.Room)
                .FirstOrDefaultAsync();
            
            if (student == null)
            {
                return null;
            }
            
            var result = MapToDto(student);
            
            // Cache for 1 hour
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting student {studentId}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Allocate room to student
    /// </summary>
    public async Task<StudentDto?> AllocateRoomAsync(int hostelId, AllocateRoomRequest request)
    {
        try
        {
            // Verify student exists
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == request.StudentId && s.HostelId == hostelId && !s.IsDeleted);
            
            if (student == null)
            {
                return null;
            }
            
            // Verify room exists and has capacity
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == request.RoomId && r.HostelId == hostelId && !r.IsDeleted);
            
            if (room == null)
            {
                return null;
            }
            
            if (room.CurrentOccupancy >= room.Capacity)
            {
                return null; // Room is full
            }
            
            // If student was already in a room, decrease that room's occupancy
            if (student.RoomId.HasValue && student.RoomId.Value != request.RoomId)
            {
                var oldRoom = await _context.Rooms.FindAsync(student.RoomId.Value);
                if (oldRoom != null && oldRoom.CurrentOccupancy > 0)
                {
                    oldRoom.CurrentOccupancy--;
                    _context.Rooms.Update(oldRoom);
                }
            }
            
            // Assign new room
            student.RoomId = request.RoomId;
            room.CurrentOccupancy++;
            
            _context.Students.Update(student);
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            
            // Invalidate caches
            await InvalidateStudentCache(student.StudentId, hostelId);
            
            _logger.LogInformation($"Room allocated to student {student.StudentId}: Room {request.RoomId}");
            
            return await GetStudentByIdAsync(student.StudentId, hostelId);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error allocating room: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Remove student from hostel (soft delete)
    /// </summary>
    public async Task<bool> RemoveStudentAsync(int studentId, int hostelId)
    {
        try
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId && s.HostelId == hostelId && !s.IsDeleted);
            
            if (student == null)
            {
                return false;
            }
            
            // Decrease room occupancy if assigned to a room
            if (student.RoomId.HasValue)
            {
                var room = await _context.Rooms.FindAsync(student.RoomId.Value);
                if (room != null && room.CurrentOccupancy > 0)
                {
                    room.CurrentOccupancy--;
                    _context.Rooms.Update(room);
                }
            }
            
            student.IsDeleted = true;
            student.UpdatedAt = DateTime.UtcNow;
            
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
            
            // Invalidate caches
            await InvalidateStudentCache(studentId, hostelId);
            
            _logger.LogInformation($"Student removed: {studentId}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error removing student {studentId}: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get students in a specific room
    /// </summary>
    public async Task<List<StudentDto>> GetStudentsByRoomAsync(int roomId, int hostelId)
    {
        try
        {
            var cacheKey = $"{STUDENT_CACHE_PREFIX}room_{roomId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<List<StudentDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var students = await _context.Students
                .Where(s => s.RoomId == roomId && s.HostelId == hostelId && !s.IsDeleted)
                .Include(s => s.User)
                .Include(s => s.Room)
                .ToListAsync();
            
            var result = students.Select(MapToDto).ToList();
            
            // Cache for 1 hour
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting students for room {roomId}: {ex.Message}");
            return new List<StudentDto>();
        }
    }
    
    private static StudentDto MapToDto(Student student)
    {
        return new StudentDto
        {
            StudentId = student.StudentId,
            UserId = student.UserId,
            StudentName = student.User?.Name ?? "Unknown",
            RollNumber = student.RollNumber,
            RoomId = student.RoomId,
            RoomNumber = student.Room?.RoomNumber,
            HostelId = student.HostelId,
            AdmissionDate = student.AdmissionDate,
            CreatedAt = student.CreatedAt
        };
    }
    
    private async Task InvalidateStudentCache(int studentId, int hostelId)
    {
        await _cache.RemoveAsync($"{STUDENT_CACHE_PREFIX}{studentId}");
        await _cache.RemoveAsync($"{STUDENT_CACHE_PREFIX}all_{hostelId}");
    }
}
