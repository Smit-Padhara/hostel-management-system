using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.DTOs.Requests;

/// <summary>
/// Request DTO for raising a complaint
/// </summary>
public class RaiseComplaintRequest
{
    /// <summary>
    /// ID of the student raising the complaint
    /// </summary>
    [Required(ErrorMessage = "Student ID is required")]
    public int StudentId { get; set; }

    /// <summary>
    /// Title of the complaint
    /// </summary>
    [Required(ErrorMessage = "Complaint title is required")]
    [StringLength(200, MinimumLength = 5,
        ErrorMessage = "Title must be between 5 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the complaint
    /// </summary>
    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000, MinimumLength = 10,
        ErrorMessage = "Description must be between 10 and 1000 characters")]
    public string Description { get; set; } = string.Empty;
}
