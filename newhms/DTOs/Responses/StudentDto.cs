namespace SmartHostelManagementSystem.DTOs.Responses;

/// <summary>
/// Response DTO for student information
/// </summary>
public class StudentDto
{
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public int HostelId { get; set; }
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public string RollNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public bool IsAssignedToRoom { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
