using SmartHostelManagementSystem.Models.DTOs;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for fee service
/// </summary>
public interface IFeeService
{
    /// <summary>
    /// Create a new fee record
    /// </summary>
    Task<FeeDto?> CreateFeeAsync(CreateFeeRequest request, int hostelId);
    
    /// <summary>
    /// Get all fees for a hostel
    /// </summary>
    Task<List<FeeDto>> GetAllFeesAsync(int hostelId);
    
    /// <summary>
    /// Get fee by ID
    /// </summary>
    Task<FeeDto?> GetFeeByIdAsync(int feeId, int hostelId);
    
    /// <summary>
    /// Get fees by student
    /// </summary>
    Task<List<FeeDto>> GetFeesByStudentAsync(int studentId, int hostelId);
    
    /// <summary>
    /// Mark fee as paid
    /// </summary>
    Task<FeeDto?> MarkAsPaidAsync(int feeId, int hostelId, MarkFeeAsPaidRequest request);
    
    /// <summary>
    /// Get fee receipt
    /// </summary>
    Task<FeeReceiptDto?> GetFeeReceiptAsync(int feeId, int hostelId);
}
