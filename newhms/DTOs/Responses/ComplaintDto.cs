namespace SmartHostelManagementSystem.DTOs.Responses;

/// <summary>
/// Response DTO for complaint information
/// </summary>
public class ComplaintDto
{
    public int ComplaintId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int HostelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, In Progress, Resolved, Closed
    public string? Resolution { get; set; }
    public DateTime ReportedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public int DaysOpen { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
