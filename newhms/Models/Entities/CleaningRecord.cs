namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// CleaningRecord entity for tracking daily room cleaning tasks
/// </summary>
public class CleaningRecord
{
    public int RecordId { get; set; }
    
    public int RoomId { get; set; }
    
    public int? WorkerId { get; set; }
    
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Status: Pending, Cleaned, Not Needed
    /// </summary>
    public string Status { get; set; } = "Pending";
    
    public string? Remarks { get; set; }
    
    public DateTime? CleanedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public virtual Room? Room { get; set; }
    public virtual Worker? Worker { get; set; }
}
