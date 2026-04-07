using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Service interface for complaint management
/// </summary>
public interface IComplaintService
{
    /// <summary>
    /// Raise a new complaint
    /// </summary>
    /// <param name="request">Complaint request</param>
    /// <returns>Created complaint DTO</returns>
    Task<ComplaintDto> RaiseComplaintAsync(RaiseComplaintRequest request);

    /// <summary>
    /// Get complaint by ID
    /// </summary>
    /// <param name="complaintId">ID of the complaint</param>
    /// <returns>Complaint DTO if found</returns>
    Task<ComplaintDto?> GetComplaintByIdAsync(int complaintId);

    /// <summary>
    /// Get all complaints in a hostel
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>List of complaints</returns>
    Task<IEnumerable<ComplaintDto>> GetComplaintsByHostelAsync(int hostelId);

    /// <summary>
    /// Get complaints by specific status
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <param name="status">Status to filter by (Pending, In Progress, Resolved, Closed)</param>
    /// <returns>List of filtered complaints</returns>
    Task<IEnumerable<ComplaintDto>> GetComplaintsByStatusAsync(int hostelId, string status);

    /// <summary>
    /// Get all complaints from a specific student
    /// </summary>
    /// <param name="studentId">ID of the student</param>
    /// <returns>List of student's complaints</returns>
    Task<IEnumerable<ComplaintDto>> GetComplaintsByStudentAsync(int studentId);

    /// <summary>
    /// Update complaint status
    /// </summary>
    /// <param name="complaintId">ID of the complaint</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated complaint DTO</returns>
    Task<ComplaintDto?> UpdateComplaintStatusAsync(int complaintId, UpdateComplaintStatusRequest request);

    /// <summary>
    /// Get pending complaints count
    /// </summary>
    /// <param name="hostelId">ID of the hostel</param>
    /// <returns>Count of pending complaints</returns>
    Task<int> GetPendingComplaintsCountAsync(int hostelId);

    /// <summary>
    /// Delete complaint (soft delete)
    /// </summary>
    /// <param name="complaintId">ID of the complaint</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteComplaintAsync(int complaintId);
}
