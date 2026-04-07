using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.DTOs.Requests;

/// <summary>
/// Request DTO for recording fees
/// </summary>
public class RecordFeeRequest
{
    /// <summary>
    /// ID of the student for whom fee is recorded
    /// </summary>
    [Required(ErrorMessage = "Student ID is required")]
    public int StudentId { get; set; }

    /// <summary>
    /// Amount of the fee
    /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Description/reason for the fee
    /// </summary>
    [StringLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// Payment status (Pending, Paid, Partial)
    /// </summary>
    [RegularExpression("^(Pending|Paid|Partial)$",
        ErrorMessage = "Payment status must be one of: Pending, Paid, Partial")]
    public string PaymentStatus { get; set; } = "Pending";

    /// <summary>
    /// Due date for the fee
    /// </summary>
    public DateTime? DueDate { get; set; }
}
