namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Represents a worker (cleaning staff) in a hostel
/// </summary>
public class Worker
{
    public int WorkerId { get; set; }
    
    public int UserId { get; set; }
    
    public int HostelId { get; set; }
    
    public string Department { get; set; } = "Cleaning"; // "Cleaning", "Maintenance", "Security"
    
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public User? User { get; set; }
    public Hostel? Hostel { get; set; }
    public ICollection<CleaningRecord>? CleaningRecords { get; set; }
}
