using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// API Controller for complaint management operations
/// Handles complaint creation, status updates, and tracking
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ComplaintController : ControllerBase
{
    private readonly IComplaintService _complaintService;
    private readonly ILogger<ComplaintController> _logger;

    public ComplaintController(IComplaintService complaintService, ILogger<ComplaintController> logger)
    {
        _complaintService = complaintService;
        _logger = logger;
    }

    /// <summary>
    /// Raise a new complaint
    /// POST: api/complaint
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ComplaintDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ComplaintDto>> RaiseComplaint([FromBody] RaiseComplaintRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var complaint = await _complaintService.RaiseComplaintAsync(request);
            _logger.LogInformation($"Complaint raised by student {request.StudentId}");
            
            return CreatedAtAction(nameof(GetComplaintById), new { id = complaint.ComplaintId }, complaint);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Failed to raise complaint: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error raising complaint: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while raising the complaint" });
        }
    }

    /// <summary>
    /// Get complaint by ID
    /// GET: api/complaint/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ComplaintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComplaintDto>> GetComplaintById(int id)
    {
        try
        {
            var complaint = await _complaintService.GetComplaintByIdAsync(id);
            
            if (complaint == null)
                return NotFound(new { message = $"Complaint with ID {id} not found" });

            return Ok(complaint);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching complaint {id}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching the complaint" });
        }
    }

    /// <summary>
    /// Get all complaints in a hostel
    /// GET: api/complaint/hostel/{hostelId}
    /// </summary>
    [HttpGet("hostel/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<ComplaintDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetComplaintsByHostel(int hostelId)
    {
        try
        {
            var complaints = await _complaintService.GetComplaintsByHostelAsync(hostelId);
            return Ok(complaints);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching complaints for hostel {hostelId}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching complaints" });
        }
    }

    /// <summary>
    /// Get complaints filtered by status
    /// GET: api/complaint/hostel/{hostelId}/status/{status}
    /// Status: Pending, In Progress, Resolved, Closed
    /// </summary>
    [HttpGet("hostel/{hostelId}/status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<ComplaintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetComplaintsByStatus(int hostelId, string status)
    {
        try
        {
            var complaints = await _complaintService.GetComplaintsByStatusAsync(hostelId, status);
            return Ok(complaints);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching complaints by status: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching complaints" });
        }
    }

    /// <summary>
    /// Get complaints from a specific student
    /// GET: api/complaint/student/{studentId}
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(IEnumerable<ComplaintDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetComplaintsByStudent(int studentId)
    {
        try
        {
            var complaints = await _complaintService.GetComplaintsByStudentAsync(studentId);
            return Ok(complaints);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching complaints for student {studentId}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching complaints" });
        }
    }

    /// <summary>
    /// Update complaint status
    /// PUT: api/complaint/{id}/status
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(ComplaintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComplaintDto>> UpdateComplaintStatus(
        int id, 
        [FromBody] UpdateComplaintStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var complaint = await _complaintService.UpdateComplaintStatusAsync(id, request);
            
            if (complaint == null)
                return NotFound(new { message = $"Complaint with ID {id} not found" });

            _logger.LogInformation($"Complaint {id} status updated to {request.Status}");
            return Ok(complaint);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating complaint status: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the complaint" });
        }
    }

    /// <summary>
    /// Get pending complaints count
    /// GET: api/complaint/pending-count/{hostelId}
    /// </summary>
    [HttpGet("pending-count/{hostelId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPendingCount(int hostelId)
    {
        try
        {
            var count = await _complaintService.GetPendingComplaintsCountAsync(hostelId);
            return Ok(new { hostelId = hostelId, pendingComplaints = count });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting pending complaints count: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Delete complaint (soft delete)
    /// DELETE: api/complaint/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComplaint(int id)
    {
        try
        {
            var result = await _complaintService.DeleteComplaintAsync(id);
            
            if (!result)
                return NotFound(new { message = $"Complaint with ID {id} not found" });

            _logger.LogInformation($"Complaint {id} deleted successfully");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting complaint: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the complaint" });
        }
    }
}
