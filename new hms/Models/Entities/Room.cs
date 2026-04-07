namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Room entity representing a hostel room
/// </summary>
public class Room
{
    public int RoomId { get; set; }
    
    public int HostelId { get; set; }
    
    public string RoomNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum capacity of the room
    /// </summary>
    public int Capacity { get; set; }
    
    /// <summary>
    /// Current occupancy count
    /// </summary>
    public int CurrentOccupancy { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public virtual Hostel? Hostel { get; set; }
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<CleaningRecord> CleaningRecords { get; set; } = new List<CleaningRecord>();
}
