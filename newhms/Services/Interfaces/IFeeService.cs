using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Service interface for fee and payment management
/// </summary>
public interface IFeeService
{
    /// <summary>
    /// Record a new fee for a student
    /// </summary>
    /// <param name="request">Fee recording request</param>
    /// <returns>Created fee DTO</returns>
    Task<FeeDto> RecordFeeAsync(RecordFeeRequest request);

    /// <summary>
    /// Get fee by ID
    /// </summary>
    /// <param name="feeId">ID of the fee</param>
    /// <returns>Fee DTO if found</returns>
    Task<FeeDto?> GetFeeByIdAsync(int feeId);

    /// <summary>
    /// Get all fees for a student
    /// </summary>
    /// <param name="studentId">ID of the student</param>
    /// <returns>List of fees for the student</returns>
    Task<IEnumerable<FeeDto>> GetFeesByStudentAsync(int studentId);

    /// <summary>
    /// Get all fees in a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of all fees in the hostel</returns>
    Task<IEnumerable<FeeDto>> GetFeesByHostelAsync(int hostelId);

    /// <summary>
    /// Get pending fees in a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of pending fees</returns>
    Task<IEnumerable<FeeDto>> GetPendingFeesAsync(int hostelId);

    /// <summary>
    /// Get overdue fees
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of overdue fees</returns>
    Task<IEnumerable<FeeDto>> GetOverdueFeesAsync(int hostelId);

    /// <summary>
    /// Record a payment for a fee
    /// </summary>
    /// <param name="feeId">ID of the fee</param>
    /// <param name="paymentAmount">Amount paid</param>
    /// <param name="paymentDate">Date of payment</param>
    /// <returns>Updated fee DTO and receipt number</returns>
    Task<(FeeDto feeDto, string receiptNumber)> RecordPaymentAsync(int feeId, decimal paymentAmount, DateTime paymentDate);

    /// <summary>
    /// Get total pending fees amount for a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Total pending fees amount</returns>
    Task<decimal> GetTotalPendingFeesAsync(int hostelId);

    /// <summary>
    /// Get total fees collected for a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Total fees collected</returns>
    Task<decimal> GetTotalFeesCollectedAsync(int hostelId);

    /// <summary>
    /// Get count of students with pending fees
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Number of students with pending fees</returns>
    Task<int> GetStudentsWithPendingFeesCountAsync(int hostelId);

    /// <summary>
    /// Generate receipt details
    /// </summary>
    /// <param name="feeId">ID of the fee</param>
    /// <param name="paymentAmount">Amount paid</param>
    /// <param name="paymentDate">Date of payment</param>
    /// <returns>Receipt details as dictionary</returns>
    Task<Dictionary<string, object>> GenerateReceiptAsync(int feeId, decimal paymentAmount, DateTime paymentDate);
}
