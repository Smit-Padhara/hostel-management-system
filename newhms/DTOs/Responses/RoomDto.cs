namespace SmartHostelManagementSystem.DTOs.Responses;

/// <summary>
/// Response DTO for room information with availability details
/// </summary>
public class RoomDto
{
    public int RoomId { get; set; }
    public int HostelId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int AvailableSeats { get; set; }
    public bool IsFull { get; set; }
    public bool IsEmpty { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
