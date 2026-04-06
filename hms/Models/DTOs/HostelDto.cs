using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.Models.DTOs;

/// <summary>
/// DTO for creating/updating a hostel
/// </summary>
public class CreateHostelRequest
{
    [Required(ErrorMessage = "Hostel name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
    public string Name { get; set; } = null!;
    
    [Required(ErrorMessage = "Location is required")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Location must be between 5 and 200 characters")]
    public string Location { get; set; } = null!;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Capacity is required")]
    [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10000")]
    public int Capacity { get; set; }
}

/// <summary>
/// DTO for hostel information
/// </summary>
public class HostelDto
{
    public int HostelId { get; set; }
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public int TotalRooms { get; set; }
    public int TotalStudents { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for hostel statistics
/// </summary>
public class HostelStatsDto
{
    public int HostelId { get; set; }
    public string HostelName { get; set; } = null!;
    public int TotalRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int TotalStudents { get; set; }
    public int TotalComplaints { get; set; }
    public int PendingComplaints { get; set; }
    public decimal TotalFeesPending { get; set; }
}
