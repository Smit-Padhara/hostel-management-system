using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.Models.DTOs;

/// <summary>
/// DTO for creating/updating a room
/// </summary>
public class CreateRoomRequest
{
    [Required(ErrorMessage = "Room number is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Room number must be between 1 and 50 characters")]
    public string RoomNumber { get; set; } = null!;
    
    [Required(ErrorMessage = "Capacity is required")]
    [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100")]
    public int Capacity { get; set; }
    
    [Required(ErrorMessage = "Hostel ID is required")]
    public int HostelId { get; set; }
}

/// <summary>
/// DTO for room information
/// </summary>
public class RoomDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int AvailableSpots => Capacity - CurrentOccupancy;
    public int HostelId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for room availability
/// </summary>
public class RoomAvailabilityDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public int AvailableSpots { get; set; }
    public bool IsAvailable => AvailableSpots > 0;
}
