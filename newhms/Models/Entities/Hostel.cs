namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Hostel entity representing a hostel location on the platform
/// </summary>
public class Hostel
{
    public int HostelId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Location { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public int Capacity { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag - if true, hostel is logically deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public virtual ICollection<Fee> Fees { get; set; } = new List<Fee>();
    public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
}
