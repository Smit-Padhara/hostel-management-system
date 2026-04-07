using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Implementation of IComplaintService for complaint management and tracking
/// </summary>
public class ComplaintService : IComplaintService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private const string COMPLAINT_CACHE_PREFIX = "complaint_";
    private const string COMPLAINTS_HOSTEL_CACHE_PREFIX = "complaints_hostel_";

    public ComplaintService(ApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Raise a new complaint with validation
    /// </summary>
    public async Task<ComplaintDto> RaiseComplaintAsync(RaiseComplaintRequest request)
    {
        // Validate: Student exists
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == request.StudentId && !s.IsDeleted);

        if (student == null)
            throw new InvalidOperationException($"Student with ID {request.StudentId} not found.");

        var complaint = new Complaint
        {
            StudentId = request.StudentId,
            HostelId = student.HostelId, // Business Rule: Filter by HostelId
            Title = request.Title,
            Description = request.Description,
            Status = "Pending",
            ReportedDate = DateTime.UtcNow,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"{COMPLAINTS_HOSTEL_CACHE_PREFIX}{student.HostelId}");

        return MapToComplaintDto(complaint, student);
    }

    /// <summary>
    /// Get complaint by ID with caching
    /// </summary>
    public async Task<ComplaintDto?> GetComplaintByIdAsync(int complaintId)
    {
        // Try cache first
        var cachedData = await _cacheService.GetAsync<ComplaintDto>($"{COMPLAINT_CACHE_PREFIX}{complaintId}");
        if (cachedData != null)
            return cachedData;

        var complaint = await _context.Complaints
            .Include(c => c.Student)
            .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId && !c.IsDeleted);

        if (complaint == null)
            return null;

        var dto = MapToComplaintDto(complaint, complaint.Student);
        
        // Cache for 30 minutes
        await _cacheService.SetAsync($"{COMPLAINT_CACHE_PREFIX}{complaintId}", dto, TimeSpan.FromMinutes(30));

        return dto;
    }

    /// <summary>
    /// Get all complaints in a hostel with caching
    /// </summary>
    public async Task<IEnumerable<ComplaintDto>> GetComplaintsByHostelAsync(int hostelId)
    {
        // Try cache first
        var cachedData = await _cacheService.GetAsync<IEnumerable<ComplaintDto>>(
            $"{COMPLAINTS_HOSTEL_CACHE_PREFIX}{hostelId}");
        if (cachedData != null)
            return cachedData;

        var complaints = await _context.Complaints
            .Include(c => c.Student)
            .ThenInclude(s => s!.User)
            .Where(c => c.HostelId == hostelId && !c.IsDeleted)
            .OrderByDescending(c => c.ReportedDate)
            .ToListAsync();

        var dtos = complaints.Select(c => MapToComplaintDto(c, c.Student)).ToList();

        // Cache for 15 minutes
        await _cacheService.SetAsync(
            $"{COMPLAINTS_HOSTEL_CACHE_PREFIX}{hostelId}", 
            dtos, 
            TimeSpan.FromMinutes(15));

        return dtos;
    }

    /// <summary>
    /// Get complaints filtered by status
    /// </summary>
    public async Task<IEnumerable<ComplaintDto>> GetComplaintsByStatusAsync(int hostelId, string status)
    {
        // Validate status
        var validStatuses = new[] { "Pending", "In Progress", "Resolved", "Closed" };
        if (!validStatuses.Contains(status))
            throw new ArgumentException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");

        var complaints = await _context.Complaints
            .Include(c => c.Student)
            .ThenInclude(s => s!.User)
            .Where(c => 
                c.HostelId == hostelId && 
                c.Status == status &&
                !c.IsDeleted)
            .OrderByDescending(c => c.ReportedDate)
            .ToListAsync();

        return complaints.Select(c => MapToComplaintDto(c, c.Student)).ToList();
    }

    /// <summary>
    /// Get all complaints from a specific student
    /// </summary>
    public async Task<IEnumerable<ComplaintDto>> GetComplaintsByStudentAsync(int studentId)
    {
        var complaints = await _context.Complaints
            .Include(c => c.Student)
            .ThenInclude(s => s!.User)
            .Where(c => c.StudentId == studentId && !c.IsDeleted)
            .OrderByDescending(c => c.ReportedDate)
            .ToListAsync();

        return complaints.Select(c => MapToComplaintDto(c, c.Student)).ToList();
    }

    /// <summary>
    /// Update complaint status with resolution tracking
    /// </summary>
    public async Task<ComplaintDto?> UpdateComplaintStatusAsync(int complaintId, UpdateComplaintStatusRequest request)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Student)
            .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId && !c.IsDeleted);

        if (complaint == null)
            return null;

        var previousStatus = complaint.Status;
        complaint.Status = request.Status;

        // Business Rule: Track resolution timestamp
        if (request.Status == "Resolved" && previousStatus != "Resolved")
        {
            complaint.ResolvedDate = DateTime.UtcNow;
            complaint.Resolution = request.Resolution;
        }

        complaint.UpdatedAt = DateTime.UtcNow;

        _context.Complaints.Update(complaint);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"{COMPLAINT_CACHE_PREFIX}{complaintId}");
        await _cacheService.RemoveAsync($"{COMPLAINTS_HOSTEL_CACHE_PREFIX}{complaint.HostelId}");

        return MapToComplaintDto(complaint, complaint.Student);
    }

    /// <summary>
    /// Get count of pending complaints
    /// </summary>
    public async Task<int> GetPendingComplaintsCountAsync(int hostelId)
    {
        return await _context.Complaints
            .CountAsync(c => 
                c.HostelId == hostelId && 
                c.Status == "Pending" &&
                !c.IsDeleted);
    }

    /// <summary>
    /// Delete complaint (soft delete)
    /// </summary>
    public async Task<bool> DeleteComplaintAsync(int complaintId)
    {
        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.ComplaintId == complaintId);

        if (complaint == null)
            return false;

        complaint.IsDeleted = true;
        complaint.UpdatedAt = DateTime.UtcNow;

        _context.Complaints.Update(complaint);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"{COMPLAINT_CACHE_PREFIX}{complaintId}");
        await _cacheService.RemoveAsync($"{COMPLAINTS_HOSTEL_CACHE_PREFIX}{complaint.HostelId}");

        return true;
    }

    /// <summary>
    /// Helper to map Complaint entity to DTO
    /// </summary>
    private static ComplaintDto MapToComplaintDto(Complaint complaint, Student? student)
    {
        var daysOpen = (complaint.ResolvedDate ?? DateTime.UtcNow) - complaint.ReportedDate;

        return new ComplaintDto
        {
            ComplaintId = complaint.ComplaintId,
            StudentId = complaint.StudentId,
            StudentName = student?.User?.FullName ?? "Unknown",
            HostelId = complaint.HostelId,
            Title = complaint.Title,
            Description = complaint.Description,
            Status = complaint.Status,
            Resolution = complaint.Resolution,
            ReportedDate = complaint.ReportedDate,
            ResolvedDate = complaint.ResolvedDate,
            DaysOpen = (int)daysOpen.TotalDays,
            CreatedAt = complaint.CreatedAt,
            UpdatedAt = complaint.UpdatedAt
        };
    }
}
