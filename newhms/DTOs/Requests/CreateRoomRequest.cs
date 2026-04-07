using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new room
/// </summary>
public class CreateRoomRequest
{
    /// <summary>
    /// ID of the hostel this room belongs to
    /// </summary>
    [Required(ErrorMessage = "Hostel ID is required")]
    public int HostelId { get; set; }

    /// <summary>
    /// Room number/identifier
    /// </summary>
    [Required(ErrorMessage = "Room number is required")]
    [StringLength(50, MinimumLength = 1, 
        ErrorMessage = "Room number must be between 1 and 50 characters")]
    public string RoomNumber { get; set; } = string.Empty;

    /// <summary>
    /// Maximum capacity of the room
    /// </summary>
    [Range(1, 50, ErrorMessage = "Room capacity must be between 1 and 50")]
    public int Capacity { get; set; }
}
