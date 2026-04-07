namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Complaint entity for tracking student complaints
/// </summary>
public class Complaint
{
    public int ComplaintId { get; set; }
    
    public int StudentId { get; set; }
    
    public int HostelId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Status: Pending, In Progress, Resolved, Closed
    /// </summary>
    public string Status { get; set; } = "Pending";
    
    public string? Resolution { get; set; }
    
    public DateTime ReportedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ResolvedDate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public virtual Student? Student { get; set; }
    public virtual Hostel? Hostel { get; set; }
}
