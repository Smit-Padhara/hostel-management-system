namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Student entity representing a hostel student
/// </summary>
public class Student
{
    public int StudentId { get; set; }
    
    public int UserId { get; set; }
    
    public int HostelId { get; set; }
    
    public int? RoomId { get; set; }
    
    public string RollNumber { get; set; } = string.Empty;
    
    public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    public virtual Room? Room { get; set; }
    public virtual Hostel? Hostel { get; set; }
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public virtual ICollection<Fee> Fees { get; set; } = new List<Fee>();
}
