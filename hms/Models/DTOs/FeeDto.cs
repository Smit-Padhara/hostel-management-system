using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.Models.DTOs;

/// <summary>
/// DTO for creating a fee record
/// </summary>
public class CreateFeeRequest
{
    [Required(ErrorMessage = "Student ID is required")]
    public int StudentId { get; set; }
    
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required(ErrorMessage = "Due date is required")]
    public DateTime DueDate { get; set; }
}

/// <summary>
/// DTO for updating fee payment
/// </summary>
public class MarkFeeAsPaidRequest
{
    [Required(ErrorMessage = "Transaction ID is required")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Transaction ID must be between 5 and 100 characters")]
    public string TransactionId { get; set; } = null!;
}

/// <summary>
/// DTO for fee information
/// </summary>
public class FeeDto
{
    public int FeeId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for fee receipt (download)
/// </summary>
public class FeeReceiptDto
{
    public int FeeId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = null!;
    public string HostelName { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PaidDate { get; set; }
    public string TransactionId { get; set; } = null!;
}
