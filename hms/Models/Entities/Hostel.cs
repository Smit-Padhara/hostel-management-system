namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Represents a hostel in the system
/// </summary>
public class Hostel
{
    public int HostelId { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string Location { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public int Capacity { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public ICollection<User>? Users { get; set; }
    public ICollection<Room>? Rooms { get; set; }
    public ICollection<Student>? Students { get; set; }
    public ICollection<Complaint>? Complaints { get; set; }
    public ICollection<Fee>? Fees { get; set; }
    public ICollection<Worker>? Workers { get; set; }
}
