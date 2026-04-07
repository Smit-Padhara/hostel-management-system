namespace SmartHostelManagementSystem.DTOs.Responses;

/// <summary>
/// Response DTO for fee/payment information
/// </summary>
public class FeeDto
{
    public int FeeId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int HostelId { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty; // Pending, Paid, Partial
    public DateTime DueDate { get; set; }
    public bool IsOverdue { get; set; }
    public string? ReceiptNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
