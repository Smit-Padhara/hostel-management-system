using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// Cleaning Management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CleaningController : ControllerBase
{
    private readonly ICleaningService _cleaningService;
    private readonly ILogger<CleaningController> _logger;
    
    public CleaningController(ICleaningService cleaningService, ILogger<CleaningController> logger)
    {
        _cleaningService = cleaningService;
        _logger = logger;
    }
    
    private int GetHostelIdFromToken()
    {
        var hostelIdClaim = User.FindFirst("HostelId");
        return int.TryParse(hostelIdClaim?.Value, out var hostelId) ? hostelId : 0;
    }
    
    private int GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst("UserId");
        return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
    }
    
    /// <summary>
    /// Mark room as cleaned/pending (Worker only)
    /// </summary>
    [HttpPost("mark")]
    [Authorize(Roles = "Worker")]
    public async Task<ActionResult<ApiResponse<CleaningRecordDto>>> MarkAsCleanedAsync([FromBody] MarkCleaningRequest request)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            
            // Get worker ID (need to query the worker entity)
            _logger.LogInformation($"Marking room as cleaned: {request.RoomId}");
            
            // For now, we'll need to pass the worker ID - in production, get from claims
            // This would require getting the Worker entity based on UserId
            var result = await _cleaningService.MarkAsCleanedAsync(0, hostelId, request);
            
            if (result == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to mark room as cleaned"
                });
            }
            
            return Ok(new ApiResponse<CleaningRecordDto>
            {
                Success = true,
                Message = "Room cleaning status updated successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking cleaning: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to mark room as cleaned"
            });
        }
    }
    
    /// <summary>
    /// Get today's cleaning report (Admin or Worker)
    /// </summary>
    [HttpGet("today")]
    [Authorize(Roles = "Admin,Worker")]
    public async Task<ActionResult<ApiResponse<DailyCleaningReportDto>>> GetTodayCleaningReportAsync()
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching today's cleaning report for hostel: {hostelId}");
            
            var result = await _cleaningService.GetTodayCleaningReportAsync(hostelId);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unable to generate cleaning report"
                });
            }
            
            return Ok(new ApiResponse<DailyCleaningReportDto>
            {
                Success = true,
                Message = "Today's cleaning report retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting cleaning report: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve cleaning report"
            });
        }
    }
    
    /// <summary>
    /// Get pending cleaning rooms (Admin or Worker)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Worker")]
    public async Task<ActionResult<ApiResponse<List<PendingCleaningDto>>>> GetPendingCleaningAsync()
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching pending cleaning for hostel: {hostelId}");
            
            var result = await _cleaningService.GetPendingCleaningAsync(hostelId);
            
            return Ok(new ApiResponse<List<PendingCleaningDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} pending cleaning tasks",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting pending cleaning: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve pending cleaning"
            });
        }
    }
    
    /// <summary>
    /// Get cleaning history for a room
    /// </summary>
    [HttpGet("history/room/{roomId}")]
    [Authorize(Roles = "Admin,Worker")]
    public async Task<ActionResult<ApiResponse<List<CleaningRecordDto>>>> GetRoomCleaningHistoryAsync(int roomId, [FromQuery] int days = 30)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching cleaning history for room: {roomId}");
            
            var result = await _cleaningService.GetRoomCleaningHistoryAsync(roomId, hostelId, days);
            
            return Ok(new ApiResponse<List<CleaningRecordDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} cleaning records",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting cleaning history: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve cleaning history"
            });
        }
    }
    
    /// <summary>
    /// Get cleaning records for a date range (Admin only)
    /// </summary>
    [HttpGet("date-range")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<CleaningRecordDto>>>> GetCleaningRecordsByDateRangeAsync([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching cleaning records from {startDate} to {endDate}");
            
            var result = await _cleaningService.GetCleaningRecordsByDateRangeAsync(hostelId, startDate, endDate);
            
            return Ok(new ApiResponse<List<CleaningRecordDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} cleaning records",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting cleaning records: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve cleaning records"
            });
        }
    }
}
