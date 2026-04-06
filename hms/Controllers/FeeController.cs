using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// Fee Management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class FeeController : ControllerBase
{
    private readonly IFeeService _feeService;
    private readonly ILogger<FeeController> _logger;
    
    public FeeController(IFeeService feeService, ILogger<FeeController> logger)
    {
        _feeService = feeService;
        _logger = logger;
    }
    
    private int GetHostelIdFromToken()
    {
        var hostelIdClaim = User.FindFirst("HostelId");
        return int.TryParse(hostelIdClaim?.Value, out var hostelId) ? hostelId : 0;
    }
    
    /// <summary>
    /// Create a new fee record (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<FeeDto>>> CreateFeeAsync([FromBody] CreateFeeRequest request)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Creating fee for student: {request.StudentId}");
            
            var result = await _feeService.CreateFeeAsync(request, hostelId);
            
            if (result == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create fee"
                });
            }
            
            return Created(string.Empty, new ApiResponse<FeeDto>
            {
                Success = true,
                Message = "Fee created successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating fee: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to create fee"
            });
        }
    }
    
    /// <summary>
    /// Get all fees for a hostel (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<FeeDto>>>> GetAllFeesAsync()
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching fees for hostel: {hostelId}");
            
            var result = await _feeService.GetAllFeesAsync(hostelId);
            
            return Ok(new ApiResponse<List<FeeDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} fees",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting fees: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve fees"
            });
        }
    }
    
    /// <summary>
    /// Get fee by ID
    /// </summary>
    [HttpGet("{feeId}")]
    public async Task<ActionResult<ApiResponse<FeeDto>>> GetFeeByIdAsync(int feeId)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching fee: {feeId}");
            
            var result = await _feeService.GetFeeByIdAsync(feeId, hostelId);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fee not found"
                });
            }
            
            return Ok(new ApiResponse<FeeDto>
            {
                Success = true,
                Message = "Fee retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting fee: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve fee"
            });
        }
    }
    
    /// <summary>
    /// Get fees by student (Student or Admin)
    /// </summary>
    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Admin,Student")]
    public async Task<ActionResult<ApiResponse<List<FeeDto>>>> GetFeesByStudentAsync(int studentId)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching fees for student: {studentId}");
            
            var result = await _feeService.GetFeesByStudentAsync(studentId, hostelId);
            
            return Ok(new ApiResponse<List<FeeDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} fees",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting student fees: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve fees"
            });
        }
    }
    
    /// <summary>
    /// Mark fee as paid
    /// </summary>
    [HttpPost("{feeId}/pay")]
    [Authorize(Roles = "Admin,Student")]
    public async Task<ActionResult<ApiResponse<FeeDto>>> MarkAsPaidAsync(int feeId, [FromBody] MarkFeeAsPaidRequest request)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Marking fee as paid: {feeId}");
            
            var result = await _feeService.MarkAsPaidAsync(feeId, hostelId, request);
            
            if (result == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to mark fee as paid"
                });
            }
            
            return Ok(new ApiResponse<FeeDto>
            {
                Success = true,
                Message = "Fee marked as paid successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking fee as paid: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to mark fee as paid"
            });
        }
    }
    
    /// <summary>
    /// Download fee receipt
    /// </summary>
    [HttpGet("{feeId}/receipt")]
    [Authorize(Roles = "Admin,Student")]
    public async Task<ActionResult<ApiResponse<FeeReceiptDto>>> GetFeeReceiptAsync(int feeId)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching fee receipt: {feeId}");
            
            var result = await _feeService.GetFeeReceiptAsync(feeId, hostelId);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Receipt not found"
                });
            }
            
            return Ok(new ApiResponse<FeeReceiptDto>
            {
                Success = true,
                Message = "Receipt retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting receipt: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve receipt"
            });
        }
    }
}
