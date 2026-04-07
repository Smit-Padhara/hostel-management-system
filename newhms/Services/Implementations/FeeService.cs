using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Implementation of IFeeService for fee and payment tracking with receipt generation
/// </summary>
public class FeeService : IFeeService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private const string FEE_CACHE_PREFIX = "fee_";
    private const string FEES_HOSTEL_CACHE_PREFIX = "fees_hostel_";
    private const string PENDING_FEES_CACHE_PREFIX = "pending_fees_hostel_";

    public FeeService(ApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Record a new fee for a student
    /// </summary>
    public async Task<FeeDto> RecordFeeAsync(RecordFeeRequest request)
    {
        // Validate: Student exists
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == request.StudentId && !s.IsDeleted);

        if (student == null)
            throw new InvalidOperationException($"Student with ID {request.StudentId} not found.");

        var fee = new Fee
        {
            StudentId = request.StudentId,
            HostelId = student.HostelId, // Business Rule: Filter by HostelId
            Amount = request.Amount,
            AmountPaid = request.PaymentStatus == "Paid" ? request.Amount : 0,
            Description = request.Description,
            PaymentStatus = request.PaymentStatus,
            DueDate = request.DueDate ?? DateTime.UtcNow.AddMonths(1),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Fees.Add(fee);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"{PENDING_FEES_CACHE_PREFIX}{student.HostelId}");

        return MapToFeeDto(fee, student);
    }

    /// <summary>
    /// Get fee by ID with caching
    /// </summary>
    public async Task<FeeDto?> GetFeeByIdAsync(int feeId)
    {
        // Try cache first
        var cachedData = await _cacheService.GetAsync<FeeDto>($"{FEE_CACHE_PREFIX}{feeId}");
        if (cachedData != null)
            return cachedData;

        var fee = await _context.Fees
            .Include(f => f.Student)
            .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(f => f.FeeId == feeId && !f.IsDeleted);

        if (fee == null)
            return null;

        var dto = MapToFeeDto(fee, fee.Student);
        
        // Cache for 1 hour
        await _cacheService.SetAsync($"{FEE_CACHE_PREFIX}{feeId}", dto, TimeSpan.FromHours(1));

        return dto;
    }

    /// <summary>
    /// Get all fees for a student
    /// </summary>
    public async Task<IEnumerable<FeeDto>> GetFeesByStudentAsync(int studentId)
    {
        var student = await _context.Students.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (student == null)
            return Enumerable.Empty<FeeDto>();

        var fees = await _context.Fees
            .Include(f => f.Student)
            .ThenInclude(s => s!.User)
            .Where(f => f.StudentId == studentId && !f.IsDeleted)
            .OrderByDescending(f => f.DueDate)
            .ToListAsync();

        return fees.Select(f => MapToFeeDto(f, f.Student)).ToList();
    }

    /// <summary>
    /// Get all fees in a hostel
    /// </summary>
    public async Task<IEnumerable<FeeDto>> GetFeesByHostelAsync(int hostelId)
    {
        // Try cache first
        var cachedData = await _cacheService.GetAsync<IEnumerable<FeeDto>>(
            $"{FEES_HOSTEL_CACHE_PREFIX}{hostelId}");
        if (cachedData != null)
            return cachedData;

        var fees = await _context.Fees
            .Include(f => f.Student)
            .ThenInclude(s => s!.User)
            .Where(f => f.HostelId == hostelId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var dtos = fees.Select(f => MapToFeeDto(f, f.Student)).ToList();

        // Cache for 30 minutes
        await _cacheService.SetAsync($"{FEES_HOSTEL_CACHE_PREFIX}{hostelId}", dtos, TimeSpan.FromMinutes(30));

        return dtos;
    }

    /// <summary>
    /// Get pending fees in a hostel
    /// </summary>
    public async Task<IEnumerable<FeeDto>> GetPendingFeesAsync(int hostelId)
    {
        // Try cache first
        var cachedData = await _cacheService.GetAsync<IEnumerable<FeeDto>>(
            $"{PENDING_FEES_CACHE_PREFIX}{hostelId}");
        if (cachedData != null)
            return cachedData;

        var pendingFees = await _context.Fees
            .Include(f => f.Student)
            .ThenInclude(s => s!.User)
            .Where(f => 
                f.HostelId == hostelId && 
                (f.PaymentStatus == "Pending" || f.PaymentStatus == "Partial") &&
                !f.IsDeleted)
            .OrderBy(f => f.DueDate)
            .ToListAsync();

        var dtos = pendingFees.Select(f => MapToFeeDto(f, f.Student)).ToList();

        // Cache for 15 minutes
        await _cacheService.SetAsync($"{PENDING_FEES_CACHE_PREFIX}{hostelId}", dtos, TimeSpan.FromMinutes(15));

        return dtos;
    }

    /// <summary>
    /// Get overdue fees in a hostel
    /// </summary>
    public async Task<IEnumerable<FeeDto>> GetOverdueFeesAsync(int hostelId)
    {
        var today = DateTime.UtcNow.Date;

        var overdueFees = await _context.Fees
            .Include(f => f.Student)
            .ThenInclude(s => s!.User)
            .Where(f => 
                f.HostelId == hostelId && 
                f.DueDate.Date < today &&
                (f.PaymentStatus == "Pending" || f.PaymentStatus == "Partial") &&
                !f.IsDeleted)
            .OrderBy(f => f.DueDate)
            .ToListAsync();

        return overdueFees.Select(f => MapToFeeDto(f, f.Student)).ToList();
    }

    /// <summary>
    /// Record a payment for a fee with receipt generation
    /// </summary>
    public async Task<(FeeDto feeDto, string receiptNumber)> RecordPaymentAsync(
        int feeId, 
        decimal paymentAmount, 
        DateTime paymentDate)
    {
        var fee = await _context.Fees
            .Include(f => f.Student)
            .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(f => f.FeeId == feeId && !f.IsDeleted);

        if (fee == null)
            throw new InvalidOperationException($"Fee with ID {feeId} not found.");

        if (paymentAmount <= 0)
            throw new ArgumentException("Payment amount must be greater than 0");

        var totalAmount = fee.Amount;
        var remainingAmount = totalAmount - fee.AmountPaid;

        if (paymentAmount > remainingAmount)
            throw new InvalidOperationException(
                $"Payment amount ({paymentAmount}) exceeds remaining balance ({remainingAmount})");

        // Update fee
        fee.AmountPaid += paymentAmount;
        fee.UpdatedAt = DateTime.UtcNow;

        // Business Rule: Update payment status
        if (fee.AmountPaid >= totalAmount)
        {
            fee.PaymentStatus = "Paid";
        }
        else if (fee.AmountPaid > 0)
        {
            fee.PaymentStatus = "Partial";
        }

        _context.Fees.Update(fee);
        await _context.SaveChangesAsync();

        // Generate receipt
        var receiptNumber = GenerateReceiptNumber(fee);

        // Invalidate cache
        await _cacheService.RemoveAsync($"{FEE_CACHE_PREFIX}{feeId}");
        await _cacheService.RemoveAsync($"{FEES_HOSTEL_CACHE_PREFIX}{fee.HostelId}");
        await _cacheService.RemoveAsync($"{PENDING_FEES_CACHE_PREFIX}{fee.HostelId}");

        var feeDto = MapToFeeDto(fee, fee.Student);
        return (feeDto, receiptNumber);
    }

    /// <summary>
    /// Get total pending fees amount for a hostel
    /// </summary>
    public async Task<decimal> GetTotalPendingFeesAsync(int hostelId)
    {
        var totalPending = await _context.Fees
            .Where(f => 
                f.HostelId == hostelId && 
                (f.PaymentStatus == "Pending" || f.PaymentStatus == "Partial") &&
                !f.IsDeleted)
            .SumAsync(f => f.Amount - f.AmountPaid);

        return totalPending;
    }

    /// <summary>
    /// Get total fees collected for a hostel
    /// </summary>
    public async Task<decimal> GetTotalFeesCollectedAsync(int hostelId)
    {
        var totalCollected = await _context.Fees
            .Where(f => f.HostelId == hostelId && !f.IsDeleted)
            .SumAsync(f => f.AmountPaid);

        return totalCollected;
    }

    /// <summary>
    /// Get count of students with pending fees
    /// </summary>
    public async Task<int> GetStudentsWithPendingFeesCountAsync(int hostelId)
    {
        var studentCount = await _context.Fees
            .Where(f => 
                f.HostelId == hostelId && 
                (f.PaymentStatus == "Pending" || f.PaymentStatus == "Partial") &&
                !f.IsDeleted)
            .Select(f => f.StudentId)
            .Distinct()
            .CountAsync();

        return studentCount;
    }

    /// <summary>
    /// Generate receipt details for a payment
    /// </summary>
    public async Task<Dictionary<string, object>> GenerateReceiptAsync(
        int feeId, 
        decimal paymentAmount, 
        DateTime paymentDate)
    {
        var fee = await _context.Fees
            .Include(f => f.Student)
            .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(f => f.FeeId == feeId && !f.IsDeleted);

        if (fee == null)
            throw new InvalidOperationException($"Fee with ID {feeId} not found.");

        var receiptNumber = GenerateReceiptNumber(fee);

        var receipt = new Dictionary<string, object>
        {
            { "ReceiptNumber", receiptNumber },
            { "PaymentDate", paymentDate },
            { "StudentName", fee.Student?.User?.FullName ?? "Unknown" },
            { "StudentId", fee.StudentId },
            { "RollNumber", fee.Student?.RollNumber ?? "N/A" },
            { "Description", fee.Description ?? "Hostel Fee" },
            { "Amount", fee.Amount },
            { "PaymentAmount", paymentAmount },
            { "BalanceRemaining", fee.Amount - fee.AmountPaid - paymentAmount },
            { "PaymentStatus", fee.PaymentStatus },
            { "DueDate", fee.DueDate },
            { "GeneratedAt", DateTime.UtcNow }
        };

        return receipt;
    }

    /// <summary>
    /// Helper to map Fee entity to DTO
    /// </summary>
    private static FeeDto MapToFeeDto(Fee fee, Student? student)
    {
        var remainingAmount = fee.Amount - fee.AmountPaid;
        var isOverdue = fee.DueDate.Date < DateTime.UtcNow.Date && 
                       (fee.PaymentStatus == "Pending" || fee.PaymentStatus == "Partial");

        return new FeeDto
        {
            FeeId = fee.FeeId,
            StudentId = fee.StudentId,
            StudentName = student?.User?.FullName ?? "Unknown",
            HostelId = fee.HostelId,
            Amount = fee.Amount,
            AmountPaid = fee.AmountPaid,
            RemainingAmount = remainingAmount,
            Description = fee.Description ?? "Hostel Fee",
            PaymentStatus = fee.PaymentStatus,
            DueDate = fee.DueDate,
            IsOverdue = isOverdue,
            ReceiptNumber = null,
            CreatedAt = fee.CreatedAt,
            UpdatedAt = fee.UpdatedAt
        };
    }

    /// <summary>
    /// Helper to generate unique receipt number
    /// Format: FEE-{FeeId}-{Timestamp}
    /// </summary>
    private static string GenerateReceiptNumber(Fee fee)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"FEE-{fee.FeeId:D6}-{timestamp}";
    }
}
