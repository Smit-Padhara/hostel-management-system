using SmartHostelManagementSystem.Models.DTOs;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for complaint service
/// </summary>
public interface IComplaintService
{
    /// <summary>
    /// Create a new complaint
    /// </summary>
    Task<ComplaintDto?> CreateComplaintAsync(int studentId, int hostelId, CreateComplaintRequest request);
    
    /// <summary>
    /// Get all complaints for a hostel
    /// </summary>
    Task<List<ComplaintDto>> GetAllComplaintsAsync(int hostelId);
    
    /// <summary>
    /// Get complaint by ID
    /// </summary>
    Task<ComplaintDto?> GetComplaintByIdAsync(int complaintId, int hostelId);
    
    /// <summary>
    /// Get complaints by status
    /// </summary>
    Task<List<ComplaintDto>> GetComplaintsByStatusAsync(int hostelId, string status);
    
    /// <summary>
    /// Update complaint status
    /// </summary>
    Task<ComplaintDto?> UpdateComplaintStatusAsync(int complaintId, int hostelId, UpdateComplaintRequest request);
    
    /// <summary>
    /// Delete complaint (soft delete)
    /// </summary>
    Task<bool> DeleteComplaintAsync(int complaintId, int hostelId);
}
