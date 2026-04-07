using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.DTOs.Requests;

/// <summary>
/// Request DTO for assigning a student to a room
/// </summary>
public class AssignStudentRequest
{
    /// <summary>
    /// ID of the student to assign
    /// </summary>
    [Required(ErrorMessage = "Student ID is required")]
    public int StudentId { get; set; }

    /// <summary>
    /// ID of the room to assign to
    /// </summary>
    [Required(ErrorMessage = "Room ID is required")]
    public int RoomId { get; set; }
}
