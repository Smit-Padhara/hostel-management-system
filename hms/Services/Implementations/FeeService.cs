using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;
using System.Text;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Fee service implementation
/// </summary>
public class FeeService : IFeeService
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<FeeService> _logger;
    private const string FEE_CACHE_PREFIX = "fee_";
    
    public FeeService(AppDbContext context, ICacheService cache, ILogger<FeeService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// Create a new fee record
    /// </summary>
    public async Task<FeeDto?> CreateFeeAsync(CreateFeeRequest request, int hostelId)
    {
        try
        {
            // Verify student exists
            var student = await _context.Students
                .Where(s => s.StudentId == request.StudentId && s.HostelId == hostelId && !s.IsDeleted)
                .Include(s => s.User)
                .FirstOrDefaultAsync();
            
            if (student == null)
            {
                return null;
            }
            
            var fee = new Fee
            {
                StudentId = request.StudentId,
                HostelId = hostelId,
                Amount = request.Amount,
                Status = "Pending",
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Fees.Add(fee);
            await _context.SaveChangesAsync();
            
            // Clear hostel stats cache
            await _cache.RemoveAsync($"hostel_{hostelId}_stats");
            
            _logger.LogInformation($"Fee created: {fee.FeeId}");
            
            return MapToDto(fee, student);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating fee: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get all fees for a hostel
    /// </summary>
    public async Task<List<FeeDto>> GetAllFeesAsync(int hostelId)
    {
        try
        {
            var cacheKey = $"{FEE_CACHE_PREFIX}all_{hostelId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<List<FeeDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var fees = await _context.Fees
                .Where(f => f.HostelId == hostelId && !f.IsDeleted)
                .Include(f => f.Student)
                .ThenInclude(s => s!.User)
                .ToListAsync();
            
            var result = fees.Select(f => MapToDto(f, f.Student)).ToList();
            
            // Cache for 15 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting fees for hostel {hostelId}: {ex.Message}");
            return new List<FeeDto>();
        }
    }
    
    /// <summary>
    /// Get fee by ID
    /// </summary>
    public async Task<FeeDto?> GetFeeByIdAsync(int feeId, int hostelId)
    {
        try
        {
            var cacheKey = $"{FEE_CACHE_PREFIX}{feeId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<FeeDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var fee = await _context.Fees
                .Where(f => f.FeeId == feeId && f.HostelId == hostelId && !f.IsDeleted)
                .Include(f => f.Student)
                .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync();
            
            if (fee == null)
            {
                return null;
            }
            
            var result = MapToDto(fee, fee.Student);
            
            // Cache for 1 hour
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting fee {feeId}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get fees by student
    /// </summary>
    public async Task<List<FeeDto>> GetFeesByStudentAsync(int studentId, int hostelId)
    {
        try
        {
            var cacheKey = $"{FEE_CACHE_PREFIX}student_{studentId}";
            
            // Try to get from cache
            var cached = await _cache.GetAsync<List<FeeDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }
            
            var fees = await _context.Fees
                .Where(f => f.StudentId == studentId && f.HostelId == hostelId && !f.IsDeleted)
                .Include(f => f.Student)
                .ThenInclude(s => s!.User)
                .ToListAsync();
            
            var result = fees.Select(f => MapToDto(f, f.Student)).ToList();
            
            // Cache for 30 minutes
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting fees for student {studentId}: {ex.Message}");
            return new List<FeeDto>();
        }
    }
    
    /// <summary>
    /// Mark fee as paid
    /// </summary>
    public async Task<FeeDto?> MarkAsPaidAsync(int feeId, int hostelId, MarkFeeAsPaidRequest request)
    {
        try
        {
            var fee = await _context.Fees
                .Where(f => f.FeeId == feeId && f.HostelId == hostelId && !f.IsDeleted)
                .Include(f => f.Student)
                .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync();
            
            if (fee == null)
            {
                return null;
            }
            
            fee.Status = "Paid";
            fee.PaidDate = DateTime.UtcNow;
            fee.TransactionId = request.TransactionId;
            fee.UpdatedAt = DateTime.UtcNow;
            
            _context.Fees.Update(fee);
            await _context.SaveChangesAsync();
            
            // Clear cache
            await InvalidateFeeCache(feeId, fee.StudentId, hostelId);
            await _cache.RemoveAsync($"hostel_{hostelId}_stats");
            
            _logger.LogInformation($"Fee marked as paid: {feeId}");
            
            return MapToDto(fee, fee.Student);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking fee as paid {feeId}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get fee receipt
    /// </summary>
    public async Task<FeeReceiptDto?> GetFeeReceiptAsync(int feeId, int hostelId)
    {
        try
        {
            var fee = await _context.Fees
                .Where(f => f.FeeId == feeId && f.HostelId == hostelId && f.Status == "Paid" && !f.IsDeleted)
                .Include(f => f.Student)
                .ThenInclude(s => s!.User)
                .Include(f => f.Hostel)
                .FirstOrDefaultAsync();
            
            if (fee == null || !fee.PaidDate.HasValue)
            {
                return null;
            }
            
            return new FeeReceiptDto
            {
                FeeId = fee.FeeId,
                StudentId = fee.StudentId,
                StudentName = fee.Student?.User?.Name ?? "Unknown",
                HostelName = fee.Hostel?.Name ?? "Unknown",
                Amount = fee.Amount,
                PaidDate = fee.PaidDate.Value,
                TransactionId = fee.TransactionId ?? "N/A"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting fee receipt {feeId}: {ex.Message}");
            return null;
        }
    }
    
    private static FeeDto MapToDto(Fee fee, Student? student)
    {
        return new FeeDto
        {
            FeeId = fee.FeeId,
            StudentId = fee.StudentId,
            StudentName = student?.User?.Name ?? "Unknown",
            Amount = fee.Amount,
            Status = fee.Status,
            DueDate = fee.DueDate,
            PaidDate = fee.PaidDate,
            TransactionId = fee.TransactionId,
            CreatedAt = fee.CreatedAt
        };
    }
    
    private async Task InvalidateFeeCache(int feeId, int studentId, int hostelId)
    {
        await _cache.RemoveAsync($"{FEE_CACHE_PREFIX}{feeId}");
        await _cache.RemoveAsync($"{FEE_CACHE_PREFIX}student_{studentId}");
        await _cache.RemoveAsync($"{FEE_CACHE_PREFIX}all_{hostelId}");
    }
}
