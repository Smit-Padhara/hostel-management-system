using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new hostel
/// </summary>
public class CreateHostelRequest
{
    /// <summary>
    /// Name of the hostel
    /// </summary>
    [Required(ErrorMessage = "Hostel name is required")]
    [StringLength(100, MinimumLength = 3, 
        ErrorMessage = "Hostel name must be between 3 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Location of the hostel
    /// </summary>
    [Required(ErrorMessage = "Location is required")]
    [StringLength(200, MinimumLength = 5, 
        ErrorMessage = "Location must be between 5 and 200 characters")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Description of the hostel
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Contact phone number
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Number of rooms in the hostel
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Number of rooms must be between 1 and 1000")]
    public int TotalRooms { get; set; }
}
