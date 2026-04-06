namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Represents a complaint filed by a student
/// </summary>
public class Complaint
{
    public int ComplaintId { get; set; }
    
    public int StudentId { get; set; }
    
    public int HostelId { get; set; }
    
    public string Title { get; set; } = null!;
    
    public string Description { get; set; } = null!;
    
    public string Status { get; set; } = "Pending"; // "Pending", "In Progress", "Resolved", "Closed"
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ResolvedAt { get; set; }
    
    public string? Resolution { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public Student? Student { get; set; }
    public Hostel? Hostel { get; set; }
}
