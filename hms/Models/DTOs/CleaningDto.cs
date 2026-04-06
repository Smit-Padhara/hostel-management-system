using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.Models.DTOs;

/// <summary>
/// DTO for marking room cleaning status
/// </summary>
public class MarkCleaningRequest
{
    [Required(ErrorMessage = "Room ID is required")]
    public int RoomId { get; set; }
    
    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = null!; // "Cleaned", "Pending", "Not Needed"
    
    [StringLength(500)]
    public string? Remarks { get; set; }
}

/// <summary>
/// DTO for cleaning record information
/// </summary>
public class CleaningRecordDto
{
    public int RecordId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Status { get; set; } = null!;
    public string? Remarks { get; set; }
    public DateTime? CleanedAt { get; set; }
}

/// <summary>
/// DTO for daily cleaning report
/// </summary>
public class DailyCleaningReportDto
{
    public DateTime Date { get; set; }
    public int TotalRooms { get; set; }
    public int CleanedRooms { get; set; }
    public int PendingRooms { get; set; }
    public List<CleaningRecordDto> Records { get; set; } = new();
    public double CleaningPercentage => TotalRooms > 0 ? (CleanedRooms / (double)TotalRooms) * 100 : 0;
}

/// <summary>
/// DTO for pending cleaning rooms
/// </summary>
public class PendingCleaningDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public DateTime LastCleanedAt { get; set; }
    public int? AssignedWorkerId { get; set; }
    public string? AssignedWorkerName { get; set; }
}
