using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Implementation of IStudentService for student management with capacity validation
/// </summary>
public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _context;
    private readonly IRoomService _roomService;
    private readonly ICacheService _cacheService;
    private const string STUDENT_CACHE_PREFIX = "student_";
    private const string STUDENTS_IN_HOSTEL_CACHE_PREFIX = "students_hostel_";

    public StudentService(
        ApplicationDbContext context, 
        IRoomService roomService,
        ICacheService cacheService)
    {
        _context = context;
        _roomService = roomService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Get student by ID
    /// </summary>
    public async Task<StudentDto?> GetStudentByIdAsync(int studentId)
    {
        // Try cache first
        var cachedData = await _cacheService.GetAsync<StudentDto>($"{STUDENT_CACHE_PREFIX}{studentId}");
        if (cachedData != null)
            return cachedData;

        var student = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.StudentId == studentId && !s.IsDeleted);

        if (student == null)
            return null;

        var dto = MapToStudentDto(student);
        
        // Cache for 30 minutes
        await _cacheService.SetAsync($"{STUDENT_CACHE_PREFIX}{studentId}", dto, TimeSpan.FromMinutes(30));

        return dto;
    }

    /// <summary>
    /// Get all students in a hostel with filtering
    /// </summary>
    public async Task<IEnumerable<StudentDto>> GetStudentsByHostelAsync(int hostelId)
    {
        var students = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Room)
            .Where(s => s.HostelId == hostelId && !s.IsDeleted)
            .ToListAsync();

        return students.Select(MapToStudentDto).ToList();
    }

    /// <summary>
    /// Get all students in a specific room
    /// </summary>
    public async Task<IEnumerable<StudentDto>> GetStudentsByRoomAsync(int roomId)
    {
        var students = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Room)
            .Where(s => s.RoomId == roomId && !s.IsDeleted)
            .ToListAsync();

        return students.Select(MapToStudentDto).ToList();
    }

    /// <summary>
    /// Get all unassigned students in a hostel
    /// </summary>
    public async Task<IEnumerable<StudentDto>> GetUnassignedStudentsAsync(int hostelId)
    {
        var unassignedStudents = await _context.Students
            .Include(s => s.User)
            .Where(s => 
                s.HostelId == hostelId && 
                !s.IsDeleted &&
                s.RoomId == null) // Business Rule: Filter by HostelId for multi-tenant support
            .ToListAsync();

        return unassignedStudents.Select(MapToStudentDto).ToList();
    }

    /// <summary>
    /// Assign student to room with comprehensive validation and capacity check
    /// </summary>
    public async Task<StudentDto?> AssignStudentToRoomAsync(AssignStudentRequest request)
    {
        // Validate: Student exists
        var student = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.StudentId == request.StudentId && !s.IsDeleted);

        if (student == null)
            throw new InvalidOperationException($"Student with ID {request.StudentId} not found.");

        // Business Rule: Cannot assign already assigned student to another room
        if (student.RoomId.HasValue)
            throw new InvalidOperationException(
                $"Student is already assigned to room {student.Room?.RoomNumber}. " +
                "Unassign first before reassigning.");

        // Validate: Room exists
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.RoomId == request.RoomId && !r.IsDeleted);

        if (room == null)
            throw new InvalidOperationException($"Room with ID {request.RoomId} not found.");

        // Business Rule: Validate hostel match
        if (student.HostelId != room.HostelId)
            throw new InvalidOperationException(
                $"Student belongs to hostel {student.HostelId}, " +
                $"but room {room.RoomNumber} belongs to hostel {room.HostelId}.");

        // Business Rule: Prevent over-allocation - Critical check
        if (room.CurrentOccupancy >= room.Capacity)
            throw new InvalidOperationException(
                $"Room {room.RoomNumber} is at full capacity ({room.Capacity}/{room.Capacity}). " +
                "Cannot assign more students.");

        // Assign student to room
        student.RoomId = request.RoomId;
        student.UpdatedAt = DateTime.UtcNow;

        // Increment room occupancy
        await _roomService.IncrementOccupancyAsync(request.RoomId);

        _context.Students.Update(student);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await InvalidateStudentCache(student.HostelId, student.StudentId);

        return MapToStudentDto(student);
    }

    /// <summary>
    /// Unassign student from their room
    /// </summary>
    public async Task<bool> UnassignStudentAsync(int studentId)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == studentId && !s.IsDeleted);

        if (student == null)
            return false;

        if (!student.RoomId.HasValue)
            return true; // Already unassigned

        // Decrement room occupancy
        await _roomService.DecrementOccupancyAsync(student.RoomId.Value);

        student.RoomId = null;
        student.UpdatedAt = DateTime.UtcNow;

        _context.Students.Update(student);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await InvalidateStudentCache(student.HostelId, studentId);

        return true;
    }

    /// <summary>
    /// Get total student count in a hostel
    /// </summary>
    public async Task<int> GetStudentCountAsync(int hostelId)
    {
        return await _context.Students
            .CountAsync(s => s.HostelId == hostelId && !s.IsDeleted);
    }

    /// <summary>
    /// Check if a student is already assigned to a room
    /// </summary>
    public async Task<bool> IsStudentAssignedAsync(int studentId)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == studentId && !s.IsDeleted);

        return student?.RoomId.HasValue ?? false;
    }

    /// <summary>
    /// Helper to map Student entity to DTO
    /// </summary>
    private static StudentDto MapToStudentDto(Student student)
    {
        return new StudentDto
        {
            StudentId = student.StudentId,
            UserId = student.UserId,
            HostelId = student.HostelId,
            RoomId = student.RoomId,
            RoomNumber = student.Room?.RoomNumber,
            RollNumber = student.RollNumber,
            FullName = student.User?.FullName ?? string.Empty,
            Email = student.User?.Email ?? string.Empty,
            AdmissionDate = student.AdmissionDate,
            IsAssignedToRoom = student.RoomId.HasValue,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt
        };
    }

    /// <summary>
    /// Helper to invalidate student-related cache
    /// </summary>
    private async Task InvalidateStudentCache(int hostelId, int studentId)
    {
        await _cacheService.RemoveAsync($"{STUDENT_CACHE_PREFIX}{studentId}");
        await _cacheService.RemoveAsync($"{STUDENTS_IN_HOSTEL_CACHE_PREFIX}{hostelId}");
    }
}
