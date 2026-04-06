using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// Complaint Management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ComplaintController : ControllerBase
{
    private readonly IComplaintService _complaintService;
    private readonly ILogger<ComplaintController> _logger;
    
    public ComplaintController(IComplaintService complaintService, ILogger<ComplaintController> logger)
    {
        _complaintService = complaintService;
        _logger = logger;
    }
    
    private int GetHostelIdFromToken()
    {
        var hostelIdClaim = User.FindFirst("HostelId");
        return int.TryParse(hostelIdClaim?.Value, out var hostelId) ? hostelId : 0;
    }
    
    /// <summary>
    /// Create a new complaint (Student only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponse<ComplaintDto>>> CreateComplaintAsync([FromQuery] int studentId, [FromBody] CreateComplaintRequest request)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Creating complaint for student: {studentId}");
            
            var result = await _complaintService.CreateComplaintAsync(studentId, hostelId, request);
            
            if (result == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create complaint"
                });
            }
            
            return Created(string.Empty, new ApiResponse<ComplaintDto>
            {
                Success = true,
                Message = "Complaint created successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating complaint: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to create complaint"
            });
        }
    }
    
    /// <summary>
    /// Get all complaints for a hostel (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<ComplaintDto>>>> GetAllComplaintsAsync()
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching complaints for hostel: {hostelId}");
            
            var result = await _complaintService.GetAllComplaintsAsync(hostelId);
            
            return Ok(new ApiResponse<List<ComplaintDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} complaints",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting complaints: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve complaints"
            });
        }
    }
    
    /// <summary>
    /// Get complaint by ID
    /// </summary>
    [HttpGet("{complaintId}")]
    public async Task<ActionResult<ApiResponse<ComplaintDto>>> GetComplaintByIdAsync(int complaintId)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching complaint: {complaintId}");
            
            var result = await _complaintService.GetComplaintByIdAsync(complaintId, hostelId);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Complaint not found"
                });
            }
            
            return Ok(new ApiResponse<ComplaintDto>
            {
                Success = true,
                Message = "Complaint retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting complaint: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve complaint"
            });
        }
    }
    
    /// <summary>
    /// Get complaints by status (Admin only)
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<ComplaintDto>>>> GetComplaintsByStatusAsync(string status)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching complaints with status: {status}");
            
            var result = await _complaintService.GetComplaintsByStatusAsync(hostelId, status);
            
            return Ok(new ApiResponse<List<ComplaintDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} complaints",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting complaints by status: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve complaints"
            });
        }
    }
    
    /// <summary>
    /// Update complaint status (Admin only)
    /// </summary>
    [HttpPut("{complaintId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ComplaintDto>>> UpdateComplaintStatusAsync(int complaintId, [FromBody] UpdateComplaintRequest request)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Updating complaint: {complaintId}");
            
            var result = await _complaintService.UpdateComplaintStatusAsync(complaintId, hostelId, request);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Complaint not found"
                });
            }
            
            return Ok(new ApiResponse<ComplaintDto>
            {
                Success = true,
                Message = "Complaint updated successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating complaint: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to update complaint"
            });
        }
    }
    
    /// <summary>
    /// Delete complaint (Admin only)
    /// </summary>
    [HttpDelete("{complaintId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteComplaintAsync(int complaintId)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Deleting complaint: {complaintId}");
            
            var result = await _complaintService.DeleteComplaintAsync(complaintId, hostelId);
            
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Complaint not found"
                });
            }
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Complaint deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting complaint: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to delete complaint"
            });
        }
    }
}
