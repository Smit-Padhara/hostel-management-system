namespace SmartHostelManagementSystem.DTOs.Responses;

/// <summary>
/// Response DTO for hostel information
/// </summary>
public class HostelDto
{
    public int HostelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public int TotalRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int TotalStudents { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
