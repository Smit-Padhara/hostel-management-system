namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Represents a room in a hostel
/// </summary>
public class Room
{
    public int RoomId { get; set; }
    
    public string RoomNumber { get; set; } = null!;
    
    public int Capacity { get; set; }
    
    public int CurrentOccupancy { get; set; } = 0;
    
    public int HostelId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public Hostel? Hostel { get; set; }
    public ICollection<Student>? Students { get; set; }
    public ICollection<CleaningRecord>? CleaningRecords { get; set; }
}
