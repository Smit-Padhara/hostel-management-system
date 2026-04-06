using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.Models.DTOs;

/// <summary>
/// DTO for creating a complaint
/// </summary>
public class CreateComplaintRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
    public string Title { get; set; } = null!;
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
    public string Description { get; set; } = null!;
}

/// <summary>
/// DTO for updating complaint status
/// </summary>
public class UpdateComplaintRequest
{
    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = null!;
    
    [StringLength(500)]
    public string? Resolution { get; set; }
}

/// <summary>
/// DTO for complaint information
/// </summary>
public class ComplaintDto
{
    public int ComplaintId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
