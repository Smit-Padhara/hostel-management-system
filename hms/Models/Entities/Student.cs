namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Represents a student in the system
/// </summary>
public class Student
{
    public int StudentId { get; set; }
    
    public int UserId { get; set; }
    
    public int? RoomId { get; set; }
    
    public int HostelId { get; set; }
    
    public string? RollNumber { get; set; }
    
    public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public User? User { get; set; }
    public Room? Room { get; set; }
    public Hostel? Hostel { get; set; }
    public ICollection<Complaint>? Complaints { get; set; }
    public ICollection<Fee>? Fees { get; set; }
}
