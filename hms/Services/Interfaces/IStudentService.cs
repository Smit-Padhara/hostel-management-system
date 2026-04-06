using SmartHostelManagementSystem.Models.DTOs;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for student service
/// </summary>
public interface IStudentService
{
    /// <summary>
    /// Create a new student
    /// </summary>
    Task<StudentDto?> CreateStudentAsync(CreateStudentRequest request);
    
    /// <summary>
    /// Get all students in a hostel
    /// </summary>
    Task<List<StudentDto>> GetAllStudentsAsync(int hostelId);
    
    /// <summary>
    /// Get student by ID
    /// </summary>
    Task<StudentDto?> GetStudentByIdAsync(int studentId, int hostelId);
    
    /// <summary>
    /// Allocate room to student
    /// </summary>
    Task<StudentDto?> AllocateRoomAsync(int hostelId, AllocateRoomRequest request);
    
    /// <summary>
    /// Remove student from hostel (soft delete)
    /// </summary>
    Task<bool> RemoveStudentAsync(int studentId, int hostelId);
    
    /// <summary>
    /// Get students in a specific room
    /// </summary>
    Task<List<StudentDto>> GetStudentsByRoomAsync(int roomId, int hostelId);
}
