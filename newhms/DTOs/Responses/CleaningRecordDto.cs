namespace SmartHostelManagementSystem.DTOs.Responses;

/// <summary>
/// Response DTO for cleaning record information
/// </summary>
public class CleaningRecordDto
{
    public int RecordId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int? WorkerId { get; set; }
    public string? WorkerName { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Cleaned, Not Needed
    public string? Remarks { get; set; }
    public DateTime? CleanedAt { get; set; }
    public int? DaysOverdue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
