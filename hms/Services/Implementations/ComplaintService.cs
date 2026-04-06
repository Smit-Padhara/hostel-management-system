using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Complaint service implementation
/// </summary>
public class ComplaintService : IComplaintService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<ComplaintService> _logger;
    private const string COMPLAINT_CACHE_PREFIX = "complaint_";
    
    public ComplaintService(AppDbContext context, ICacheService cache, ILogger<ComplaintService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Create a new complaint
    /// </summary>
    public async Task<ComplaintDto?> CreateComplaintAsync(int studentId, int hostelId, CreateComplaintRequest request)
    {
        try
        {
            // Verify student exists
            var student = await _context.Students
                .Where(s => s.StudentId == studentId && s.HostelId == hostelId && !s.IsDeleted)
                .Include(s => s.User)
                .FirstOrDefaultAsync();
            
            if (student == null)
            {
                return null;
            }
            
            var complaint = new Complaint
            {
                StudentId = studentId,
                HostelId = hostelId,
                Title = request.Title,
                Description = request.Description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();
            
            // Clear hostel stats cache
            await _cache.RemoveAsync($"hostel_{hostelId}_stats");
            
            _logger.LogInformation($"Complaint created: {complaint.ComplaintId}");
            
            return MapToDto(complaint, student);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating complaint: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get all complaints for a hostel
    /// </summary>
    public async Task<List<ComplaintDto>> GetAllComplaintsAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{COMPLAINT_CACHE_PREFIX}all_{hostelId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<List<ComplaintDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var complaints = await _context.Complaints
                .Where(c => c.HostelId == hostelId && !c.IsDeleted)
                .Include(c => c.Student)
                .ThenInclude(s => s!.User)
                .ToListAsync();
            
            var result = complaints.Select(c => MapToDto(c, c.Student)).ToList();
            
            // Cache for 15 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting complaints for hostel {hostelId}: {ex.Message}");
            return new List<ComplaintDto>();
        }
    }
    
    /// <summary>
    /// Get complaint by ID
    /// </summary>
    public async Task<ComplaintDto?> GetComplaintByIdAsync(int complaintId, int hostelId)
    {
        try
        {
            var cacheKey = $"{COMPLAINT_CACHE_PREFIX}{complaintId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<ComplaintDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var complaint = await _context.Complaints
                .Where(c => c.ComplaintId == complaintId && c.HostelId == hostelId && !c.IsDeleted)
                .Include(c => c.Student)
                .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync();
            
            if (complaint == null)
            {
                return null;
            }
            
            var result = MapToDto(complaint, complaint.Student);
            
            // Cache for 1 hour
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting complaint {complaintId}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get complaints by status
    /// </summary>
    public async Task<List<ComplaintDto>> GetComplaintsByStatusAsync(int hostelId, string status)
    {
        try
        {
            var cacheKey = $"{COMPLAINT_CACHE_PREFIX}status_{hostelId}_{status}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<List<ComplaintDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var complaints = await _context.Complaints
                .Where(c => c.HostelId == hostelId && c.Status == status && !c.IsDeleted)
                .Include(c => c.Student)
                .ThenInclude(s => s!.User)
                .ToListAsync();
            
            var result = complaints.Select(c => MapToDto(c, c.Student)).ToList();
            
            // Cache for 10 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting complaints by status: {ex.Message}");
            return new List<ComplaintDto>();
        }
    }
    
    /// <summary>
    /// Update complaint status
    /// </summary>
    public async Task<ComplaintDto?> UpdateComplaintStatusAsync(int complaintId, int hostelId, UpdateComplaintRequest request)
    {
        try
        {
            var complaint = await _context.Complaints
                .Where(c => c.ComplaintId == complaintId && c.HostelId == hostelId && !c.IsDeleted)
                .Include(c => c.Student)
                .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync();
            
            if (complaint == null)
            {
                return null;
            }
            
            complaint.Status = request.Status;
            complaint.Resolution = request.Resolution;
            complaint.UpdatedAt = DateTime.UtcNow;
            
            if (request.Status == "Resolved" || request.Status == "Closed")
            {
                complaint.ResolvedAt = DateTime.UtcNow;
            }
            
            _context.Complaints.Update(complaint);
            await _context.SaveChangesAsync();
            
            // Clear cache
            await InvalidateComplaintCache(complaintId, hostelId);
            await _cache.RemoveAsync($"hostel_{hostelId}_stats");
            
            _logger.LogInformation($"Complaint updated: {complaintId}");
            
            return MapToDto(complaint, complaint.Student);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating complaint {complaintId}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Delete complaint (soft delete)
    /// </summary>
    public async Task<bool> DeleteComplaintAsync(int complaintId, int hostelId)
    {
        try
        {
            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.ComplaintId == complaintId && c.HostelId == hostelId && !c.IsDeleted);
            
            if (complaint == null)
            {
                return false;
            }
            
            complaint.IsDeleted = true;
            complaint.UpdatedAt = DateTime.UtcNow;
            
            _context.Complaints.Update(complaint);
            await _context.SaveChangesAsync();
            
            // Clear cache
            await InvalidateComplaintCache(complaintId, hostelId);
            
            _logger.LogInformation($"Complaint deleted: {complaintId}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting complaint {complaintId}: {ex.Message}");
            return false;
        }
    }
    
    private static ComplaintDto MapToDto(Complaint complaint, Student? student)
    {
        return new ComplaintDto
        {
            ComplaintId = complaint.ComplaintId,
            StudentId = complaint.StudentId,
            StudentName = student?.User?.Name ?? "Unknown",
            Title = complaint.Title,
            Description = complaint.Description,
            Status = complaint.Status,
            Resolution = complaint.Resolution,
            CreatedAt = complaint.CreatedAt,
            ResolvedAt = complaint.ResolvedAt
        };
    }
    
    private async Task InvalidateComplaintCache(int complaintId, int hostelId)
    {
        await _cache.RemoveAsync($"{COMPLAINT_CACHE_PREFIX}{complaintId}");
        await _cache.RemoveAsync($"{COMPLAINT_CACHE_PREFIX}all_{hostelId}");
    }
}
