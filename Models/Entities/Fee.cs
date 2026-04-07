using System.ComponentModel.DataAnnotations.Schema;

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
    /// Amount paid so far
    /// </summary>
    public decimal AmountPaid { get; set; }

    /// <summary>
    /// Optional fee description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Status: Pending, Paid, Partial
    /// </summary>
    public string PaymentStatus { get; set; } = "Pending";
    
    public DateTime DueDate { get; set; }
    
    public DateTime? PaidDate { get; set; }
    
    public string? TransactionId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Legacy alias for PaymentStatus
    /// </summary>
    [NotMapped]
    public string Status
    {
        get => PaymentStatus;
        set => PaymentStatus = value;
    }
    
    // Navigation properties
    public virtual Student? Student { get; set; }
    public virtual Hostel? Hostel { get; set; }
}
