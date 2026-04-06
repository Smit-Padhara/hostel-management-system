namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Represents a cleaning record for a room
/// </summary>
public class CleaningRecord
{
    public int RecordId { get; set; }
    
    public int RoomId { get; set; }
    
    public int WorkerId { get; set; }
    
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    public string Status { get; set; } = "Pending"; // "Pending", "Cleaned", "Not Needed"
    
    public string? Remarks { get; set; }
    
    public DateTime? CleanedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Room? Room { get; set; }
    public Worker? Worker { get; set; }
}
