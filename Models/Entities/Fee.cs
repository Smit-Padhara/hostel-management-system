namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Fee entity for tracking student fee payments
/// </summary>
public class Fee
{
    public int FeeId { get; set; }
    
    public int StudentId { get; set; }
    
    public int HostelId { get; set; }
    
    /// <summary>
    /// Fee amount with 2 decimal places
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Status: Pending, Paid, Overdue
    /// </summary>
    public string Status { get; set; } = "Pending";
    
    public DateTime DueDate { get; set; }
    
    public DateTime? PaidDate { get; set; }
    
    public string? TransactionId { get; set; }
    
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
