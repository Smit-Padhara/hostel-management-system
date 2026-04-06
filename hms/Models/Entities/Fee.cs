namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Represents a fee record for a student
/// </summary>
public class Fee
{
    public int FeeId { get; set; }
    
    public int StudentId { get; set; }
    
    public int HostelId { get; set; }
    
    public decimal Amount { get; set; }
    
    public string Status { get; set; } = "Pending"; // "Pending", "Paid", "Overdue"
    
    public DateTime DueDate { get; set; }
    
    public DateTime? PaidDate { get; set; }
    
    public string? TransactionId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public Student? Student { get; set; }
    public Hostel? Hostel { get; set; }
}
