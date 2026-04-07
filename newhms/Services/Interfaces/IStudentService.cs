using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Service interface for student-related operations
/// </summary>
public interface IStudentService
{
    /// <summary>
    /// Get student by ID
    /// </summary>
    /// <param name="studentId">ID of the student</param>
    /// <returns>Student DTO if found</returns>
    Task<StudentDto?> GetStudentByIdAsync(int studentId);

    /// <summary>
    /// Get all students in a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of students</returns>
    Task<IEnumerable<StudentDto>> GetStudentsByHostelAsync(int hostelId);

    /// <summary>
    /// Get all students in a specific room
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <returns>List of students in the room</returns>
    Task<IEnumerable<StudentDto>> GetStudentsByRoomAsync(int roomId);

    /// <summary>
    /// Get all unassigned students in a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of unassigned students</returns>
    Task<IEnumerable<StudentDto>> GetUnassignedStudentsAsync(int hostelId);

    /// <summary>
    /// Assign a student to a room (with capacity check and validation)
    /// </summary>
    /// <param name="request">Assignment request</param>
    /// <returns>Updated student DTO</returns>
    /// <exception cref="InvalidOperationException">Thrown when room is full or other validation fails</exception>
    Task<StudentDto?> AssignStudentToRoomAsync(AssignStudentRequest request);

    /// <summary>
    /// Unassign a student from their current room
    /// </summary>
    /// <param name="studentId">ID of the student</param>
    /// <returns>True if unassigned successfully</returns>
    Task<bool> UnassignStudentAsync(int studentId);

    /// <summary>
    /// Get total student count in a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Total number of students</returns>
    Task<int> GetStudentCountAsync(int hostelId);

    /// <summary>
    /// Check if a student is already assigned to a room
    /// </summary>
    /// <param name="studentId">ID of the student</param>
    /// <returns>True if assigned to a room</returns>
    Task<bool> IsStudentAssignedAsync(int studentId);
}
