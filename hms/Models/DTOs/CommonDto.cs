namespace SmartHostelManagementSystem.Models.DTOs;

/// <summary>
/// DTO for dashboard statistics
/// </summary>
public class AdminDashboardDto
{
    public int TotalHostels { get; set; }
    public int TotalStudents { get; set; }
    public int TotalRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int TotalComplaints { get; set; }
    public int PendingComplaints { get; set; }
    public decimal TotalFeesPending { get; set; }
    public decimal TotalFeesCollected { get; set; }
    public int PendingCleaningRooms { get; set; }
    public int CleanedRoomsToday { get; set; }
    public double CleaningPercentageToday { get; set; }
    public List<HostelStatsDto> HostelStats { get; set; } = new();
}

/// <summary>
/// DTO for API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

/// <summary>
/// DTO for pagination
/// </summary>
public class PaginationDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// DTO for paginated response
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
}
