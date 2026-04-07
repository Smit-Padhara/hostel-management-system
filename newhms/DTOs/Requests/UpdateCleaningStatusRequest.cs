using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.DTOs.Requests;

/// <summary>
/// Request DTO for updating cleaning record status
/// </summary>
public class UpdateCleaningStatusRequest
{
    /// <summary>
    /// New status for the cleaning record (Pending, Cleaned, Not Needed)
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(Pending|Cleaned|Not Needed)$",
        ErrorMessage = "Status must be one of: Pending, Cleaned, Not Needed")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional remarks about the cleaning
    /// </summary>
    [StringLength(500)]
    public string? Remarks { get; set; }

    /// <summary>
    /// ID of the worker who cleaned the room (optional)
    /// </summary>
    public int? WorkerId { get; set; }
}
