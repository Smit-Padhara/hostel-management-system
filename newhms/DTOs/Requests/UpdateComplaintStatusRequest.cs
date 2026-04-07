using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.DTOs.Requests;

/// <summary>
/// Request DTO for updating complaint status
/// </summary>
public class UpdateComplaintStatusRequest
{
    /// <summary>
    /// New status for the complaint (Pending, In Progress, Resolved, Closed)
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(Pending|In Progress|Resolved|Closed)$",
        ErrorMessage = "Status must be one of: Pending, In Progress, Resolved, Closed")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Resolution details (optional, required when resolving)
    /// </summary>
    [StringLength(500)]
    public string? Resolution { get; set; }
}
