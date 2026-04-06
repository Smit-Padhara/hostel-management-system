using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.Models.DTOs;

/// <summary>
/// DTO for student registration/update
/// </summary>
public class CreateStudentRequest
{
    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }
    
    [Required(ErrorMessage = "Hostel ID is required")]
    public int HostelId { get; set; }
    
    public int? RoomId { get; set; }
    
    [StringLength(50)]
    public string? RollNumber { get; set; }
}

/// <summary>
/// DTO for allocating student to room
/// </summary>
public class AllocateRoomRequest
{
    [Required(ErrorMessage = "Student ID is required")]
    public int StudentId { get; set; }
    
    [Required(ErrorMessage = "Room ID is required")]
    public int RoomId { get; set; }
}

/// <summary>
/// DTO for student information
/// </summary>
public class StudentDto
{
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public string StudentName { get; set; } = null!;
    public string? RollNumber { get; set; }
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public int HostelId { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
