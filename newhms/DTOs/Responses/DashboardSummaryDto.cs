namespace SmartHostelManagementSystem.DTOs.Responses;

/// <summary>
/// Response DTO for dashboard summary data
/// </summary>
public class DashboardSummaryDto
{
    /// <summary>
    /// Total number of students in the hostel
    /// </summary>
    public int TotalStudents { get; set; }

    /// <summary>
    /// Total number of rooms in the hostel
    /// </summary>
    public int TotalRooms { get; set; }

    /// <summary>
    /// Number of occupied rooms
    /// </summary>
    public int OccupiedRooms { get; set; }

    /// <summary>
    /// Number of available rooms
    /// </summary>
    public int AvailableRooms { get; set; }

    /// <summary>
    /// Number of rooms with full capacity
    /// </summary>
    public int FullRooms { get; set; }

    /// <summary>
    /// Total number of complaints (all statuses)
    /// </summary>
    public int TotalComplaints { get; set; }

    /// <summary>
    /// Number of pending complaints
    /// </summary>
    public int PendingComplaints { get; set; }

    /// <summary>
    /// Number of complaints in progress
    /// </summary>
    public int InProgressComplaints { get; set; }

    /// <summary>
    /// Number of resolved complaints
    /// </summary>
    public int ResolvedComplaints { get; set; }

    /// <summary>
    /// Number of rooms pending cleaning
    /// </summary>
    public int PendingCleaningRooms { get; set; }

    /// <summary>
    /// Number of rooms cleaned today
    /// </summary>
    public int CleanedRoomsToday { get; set; }

    /// <summary>
    /// Total outstanding fees amount
    /// </summary>
    public decimal OutstandingFeesAmount { get; set; }

    /// <summary>
    /// Number of students with pending fees
    /// </summary>
    public int StudentsWithPendingFees { get; set; }

    /// <summary>
    /// Occupancy percentage
    /// </summary>
    public decimal OccupancyPercentage { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; }
}
