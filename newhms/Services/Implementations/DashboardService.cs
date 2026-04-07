using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Implementation of IDashboardService for comprehensive dashboard analytics
/// Aggregates data from multiple services with Redis caching for performance
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly IComplaintService _complaintService;
    private readonly ICleaningService _cleaningService;
    private readonly IFeeService _feeService;
    private readonly IRoomService _roomService;
    private const string DASHBOARD_CACHE_PREFIX = "dashboard_";
    private const string DASHBOARD_ALERTS_CACHE_PREFIX = "dashboard_alerts_";

    public DashboardService(
        ApplicationDbContext context,
        ICacheService cacheService,
        IComplaintService complaintService,
        ICleaningService cleaningService,
        IFeeService feeService,
        IRoomService roomService)
    {
        _context = context;
        _cacheService = cacheService;
        _complaintService = complaintService;
        _cleaningService = cleaningService;
        _feeService = feeService;
        _roomService = roomService;
    }

    /// <summary>
    /// Get comprehensive dashboard summary with all metrics
    /// </summary>
    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int hostelId)
    {
        // Try cache first (5 minutes TTL for real-time data)
        var cacheKey = $"{DASHBOARD_CACHE_PREFIX}{hostelId}";
        var cachedData = await _cacheService.GetAsync<DashboardSummaryDto>(cacheKey);
        if (cachedData != null)
            return cachedData;

        // Fetch all data in parallel for performance
        var hostelTask = _context.Hostels.FirstOrDefaultAsync(h => h.HostelId == hostelId && !h.IsDeleted);
        var studentsTask = _context.Students.CountAsync(s => s.HostelId == hostelId && !s.IsDeleted);
        var roomsTask = _context.Rooms.Where(r => r.HostelId == hostelId && !r.IsDeleted).ToListAsync();
        var complaintsTask = _context.Complaints.Where(c => c.HostelId == hostelId && !c.IsDeleted).ToListAsync();
        var cleaningTask = _cleaningService.GetPendingCleaningCountAsync(hostelId);
        var cleanedTodayTask = GetCleanedRoomsTodayAsync(hostelId);
        var feesTask = _feeService.GetTotalPendingFeesAsync(hostelId);
        var studentsWithFeesTask = _feeService.GetStudentsWithPendingFeesCountAsync(hostelId);

        await Task.WhenAll(
            hostelTask, studentsTask, roomsTask, complaintsTask, 
            cleaningTask, cleanedTodayTask, feesTask, studentsWithFeesTask);

        var hostel = await hostelTask;
        var totalStudents = await studentsTask;
        var rooms = await roomsTask;
        var complaints = await complaintsTask;
        var pendingCleaning = await cleaningTask;
        var cleanedToday = await cleanedTodayTask;
        var outstandingFees = await feesTask;
        var studentsWithPendingFees = await studentsWithFeesTask;

        if (hostel == null)
            throw new InvalidOperationException($"Hostel with ID {hostelId} not found.");

        // Calculate metrics
        var totalRooms = rooms.Count;
        var occupiedRooms = rooms.Count(r => r.CurrentOccupancy > 0);
        var availableRooms = totalRooms - occupiedRooms;
        var fullRooms = rooms.Count(r => r.CurrentOccupancy >= r.Capacity);

        var totalComplaints = complaints.Count;
        var pendingComplaints = complaints.Count(c => c.Status == "Pending");
        var inProgressComplaints = complaints.Count(c => c.Status == "In Progress");
        var resolvedComplaints = complaints.Count(c => c.Status == "Resolved");

        var occupancyPercentage = totalRooms > 0 
            ? Math.Round((occupiedRooms / (decimal)totalRooms) * 100, 2)
            : 0;

        var summary = new DashboardSummaryDto
        {
            TotalStudents = totalStudents,
            TotalRooms = totalRooms,
            OccupiedRooms = occupiedRooms,
            AvailableRooms = availableRooms,
            FullRooms = fullRooms,
            TotalComplaints = totalComplaints,
            PendingComplaints = pendingComplaints,
            InProgressComplaints = inProgressComplaints,
            ResolvedComplaints = resolvedComplaints,
            PendingCleaningRooms = pendingCleaning,
            CleanedRoomsToday = cleanedToday,
            OutstandingFeesAmount = outstandingFees,
            StudentsWithPendingFees = studentsWithPendingFees,
            OccupancyPercentage = occupancyPercentage,
            LastUpdated = DateTime.UtcNow
        };

        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(5));

        return summary;
    }

    /// <summary>
    /// Get recent complaints for dashboard display
    /// </summary>
    public async Task<IEnumerable<ComplaintDto>> GetRecentComplaintsAsync(int hostelId, int limit = 5)
    {
        var complaints = await _context.Complaints
            .Include(c => c.Student)
            .ThenInclude(s => s!.User)
            .Where(c => c.HostelId == hostelId && !c.IsDeleted)
            .OrderByDescending(c => c.ReportedDate)
            .Take(limit)
            .ToListAsync();

        return complaints.Select(c => new ComplaintDto
        {
            ComplaintId = c.ComplaintId,
            StudentId = c.StudentId,
            StudentName = c.Student?.User?.FullName ?? "Unknown",
            HostelId = c.HostelId,
            Title = c.Title,
            Description = c.Description,
            Status = c.Status,
            Resolution = c.Resolution,
            ReportedDate = c.ReportedDate,
            ResolvedDate = c.ResolvedDate,
            DaysOpen = (int)((c.ResolvedDate ?? DateTime.UtcNow) - c.ReportedDate).TotalDays,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }

    /// <summary>
    /// Get critical alerts - High priority issues for dashboard
    /// </summary>
    public async Task<Dictionary<string, object>> GetCriticalAlertsAsync(int hostelId)
    {
        // Try cache first (2 minute TTL for alerts)
        var cacheKey = $"{DASHBOARD_ALERTS_CACHE_PREFIX}{hostelId}";
        var cachedAlerts = await _cacheService.GetAsync<Dictionary<string, object>>(cacheKey);
        if (cachedAlerts != null)
            return cachedAlerts;

        var alerts = new Dictionary<string, object>();

        // Critical: Overdue fees
        var overdueFeesCount = (await _feeService.GetOverdueFeesAsync(hostelId)).Count();
        alerts["OverdueFeesCount"] = overdueFeesCount;
        alerts["OverdueFeesCritical"] = overdueFeesCount > 5;

        // Critical: High pending complaints
        var pendingComplaintsCount = await _complaintService.GetPendingComplaintsCountAsync(hostelId);
        alerts["PendingComplaintsCount"] = pendingComplaintsCount;
        alerts["PendingComplaintsCritical"] = pendingComplaintsCount > 10;

        // Critical: Uncleaned rooms
        var uncleanlRooms = (await _cleaningService.GetUncleanlRoomsAsync(hostelId, 3)).Count();
        alerts["UncleanRoomsCount"] = uncleanlRooms;
        alerts["UncleanRoomsCritical"] = uncleanlRooms > 5;

        // Critical: Rooms at full capacity
        var rooms = await _context.Rooms
            .Where(r => r.HostelId == hostelId && !r.IsDeleted)
            .ToListAsync();
        var fullRoomsCount = rooms.Count(r => r.CurrentOccupancy >= r.Capacity);
        alerts["FullRoomsCount"] = fullRoomsCount;

        // Cache for 2 minutes
        await _cacheService.SetAsync(cacheKey, alerts, TimeSpan.FromMinutes(2));

        return alerts;
    }

    /// <summary>
    /// Get hostel utilization metrics
    /// </summary>
    public async Task<Dictionary<string, object>> GetUtilizationMetricsAsync(int hostelId)
    {
        var rooms = await _context.Rooms
            .Where(r => r.HostelId == hostelId && !r.IsDeleted)
            .ToListAsync();

        var totalRooms = rooms.Count;
        var totalCapacity = rooms.Sum(r => r.Capacity);
        var totalOccupancy = rooms.Sum(r => r.CurrentOccupancy);

        var metrics = new Dictionary<string, object>
        {
            { "TotalRooms", totalRooms },
            { "OccupiedRooms", rooms.Count(r => r.CurrentOccupancy > 0) },
            { "AvailableRooms", rooms.Count(r => r.CurrentOccupancy < r.Capacity) },
            { "FullRooms", rooms.Count(r => r.CurrentOccupancy >= r.Capacity) },
            { "TotalCapacity", totalCapacity },
            { "CurrentOccupancy", totalOccupancy },
            { "OccupancyPercentage", totalCapacity > 0 ? Math.Round((totalOccupancy / (decimal)totalCapacity) * 100, 2) : 0 },
            { "AverageRoomOccupancy", totalRooms > 0 ? Math.Round(totalOccupancy / (decimal)totalRooms, 2) : 0 }
        };

        return metrics;
    }

    /// <summary>
    /// Get complaint resolution metrics
    /// </summary>
    public async Task<Dictionary<string, object>> GetComplaintMetricsAsync(int hostelId)
    {
        var complaints = await _context.Complaints
            .Where(c => c.HostelId == hostelId && !c.IsDeleted)
            .ToListAsync();

        var totalComplaints = complaints.Count;
        var resolvedComplaints = complaints.Count(c => c.Status == "Resolved");
        var resolutionRate = totalComplaints > 0 
            ? Math.Round((resolvedComplaints / (decimal)totalComplaints) * 100, 2)
            : 0;

        // Calculate average resolution time
        var resolvedWithDates = complaints
            .Where(c => c.Status == "Resolved" && c.ResolvedDate.HasValue)
            .ToList();

        var avgResolutionTime = resolvedWithDates.Any()
            ? resolvedWithDates.Average(c => (c.ResolvedDate.Value - c.ReportedDate).TotalDays)
            : 0;

        var metrics = new Dictionary<string, object>
        {
            { "TotalComplaints", totalComplaints },
            { "PendingComplaints", complaints.Count(c => c.Status == "Pending") },
            { "InProgressComplaints", complaints.Count(c => c.Status == "In Progress") },
            { "ResolvedComplaints", resolvedComplaints },
            { "ClosedComplaints", complaints.Count(c => c.Status == "Closed") },
            { "ResolutionRate", resolutionRate },
            { "AverageResolutionTime", Math.Round(avgResolutionTime, 2) }
        };

        return metrics;
    }

    /// <summary>
    /// Get fee collection metrics
    /// </summary>
    public async Task<Dictionary<string, object>> GetFeeMetricsAsync(int hostelId)
    {
        var fees = await _context.Fees
            .Where(f => f.HostelId == hostelId && !f.IsDeleted)
            .ToListAsync();

        var totalFeeAmount = fees.Sum(f => f.Amount);
        var totalCollected = fees.Sum(f => f.AmountPaid);
        var totalPending = fees.Sum(f => f.Amount - f.AmountPaid);
        var collectionRate = totalFeeAmount > 0 
            ? Math.Round((totalCollected / totalFeeAmount) * 100, 2)
            : 0;

        var studentsWithPending = fees
            .Where(f => f.Amount > f.AmountPaid)
            .Select(f => f.StudentId)
            .Distinct()
            .Count();

        var metrics = new Dictionary<string, object>
        {
            { "TotalFeeAmount", totalFeeAmount },
            { "TotalCollected", totalCollected },
            { "TotalPending", totalPending },
            { "CollectionRate", collectionRate },
            { "PaidFees", fees.Count(f => f.PaymentStatus == "Paid") },
            { "PartialFees", fees.Count(f => f.PaymentStatus == "Partial") },
            { "PendingFees", fees.Count(f => f.PaymentStatus == "Pending") },
            { "StudentsWithPendingFees", studentsWithPending }
        };

        return metrics;
    }

    /// <summary>
    /// Cache entire dashboard data for faster retrieval
    /// </summary>
    public async Task CacheDashboardDataAsync(int hostelId)
    {
        var summary = await GetDashboardSummaryAsync(hostelId);
        var alerts = await GetCriticalAlertsAsync(hostelId);
        var utilization = await GetUtilizationMetricsAsync(hostelId);
        var complaints = await GetComplaintMetricsAsync(hostelId);
        var fees = await GetFeeMetricsAsync(hostelId);
    }

    /// <summary>
    /// Invalidate dashboard cache
    /// </summary>
    public async Task InvalidateDashboardCacheAsync(int hostelId)
    {
        await _cacheService.RemoveAsync($"{DASHBOARD_CACHE_PREFIX}{hostelId}");
        await _cacheService.RemoveAsync($"{DASHBOARD_ALERTS_CACHE_PREFIX}{hostelId}");
    }

    /// <summary>
    /// Helper to get cleaned rooms today
    /// </summary>
    private async Task<int> GetCleanedRoomsTodayAsync(int hostelId)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.CleaningRecords
            .Include(cr => cr.Room)
            .CountAsync(cr => 
                cr.Room!.HostelId == hostelId &&
                cr.Date.Date == today &&
                cr.Status == "Cleaned" &&
                !cr.IsDeleted);
    }
}
