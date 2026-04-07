namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Worker entity for cleaning and maintenance staff
/// </summary>
public class Worker
{
    public int WorkerId { get; set; }
    
    public int UserId { get; set; }
    
    public int HostelId { get; set; }
    
    public string Department { get; set; } = "Cleaning"; // Cleaning, Maintenance, etc.
    
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    public virtual Hostel? Hostel { get; set; }
    public virtual ICollection<CleaningRecord> CleaningRecords { get; set; } = new List<CleaningRecord>();
}
