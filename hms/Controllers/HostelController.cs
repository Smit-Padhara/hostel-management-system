using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Services.Interfaces;
using System.Security.Claims;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// Hostel Management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class HostelController : ControllerBase
{
    private readonly IHostelService _hostelService;
    private readonly ILogger<HostelController> _logger;
    
    public HostelController(IHostelService hostelService, ILogger<HostelController> logger)
    {
        _hostelService = hostelService;
        _logger = logger;
    }
    
    /// <summary>
    /// Create a new hostel (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<HostelDto>>> CreateHostelAsync([FromBody] CreateHostelRequest request)
    {
        try
        {
            _logger.LogInformation($"Creating hostel: {request.Name}");
            
            var result = await _hostelService.CreateHostelAsync(request);
            
            if (result == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create hostel"
                });
            }
            
            return Created(string.Empty, new ApiResponse<HostelDto>
            {
                Success = true,
                Message = "Hostel created successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating hostel: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to create hostel"
            });
        }
    }
    
    /// <summary>
    /// Get all hostels
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<HostelDto>>>> GetAllHostelsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all hostels");
            
            var result = await _hostelService.GetAllHostelsAsync();
            
            return Ok(new ApiResponse<List<HostelDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} hostels",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting hostels: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve hostels"
            });
        }
    }
    
    /// <summary>
    /// Get hostel by ID
    /// </summary>
    [HttpGet("{hostelId}")]
    public async Task<ActionResult<ApiResponse<HostelDto>>> GetHostelByIdAsync(int hostelId)
    {
        try
        {
            _logger.LogInformation($"Fetching hostel: {hostelId}");
            
            var result = await _hostelService.GetHostelByIdAsync(hostelId);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Hostel not found"
                });
            }
            
            return Ok(new ApiResponse<HostelDto>
            {
                Success = true,
                Message = "Hostel retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting hostel: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve hostel"
            });
        }
    }
    
    /// <summary>
    /// Get hostel statistics
    /// </summary>
    [HttpGet("{hostelId}/stats")]
    public async Task<ActionResult<ApiResponse<HostelStatsDto>>> GetHostelStatsAsync(int hostelId)
    {
        try
        {
            _logger.LogInformation($"Fetching hostel stats: {hostelId}");
            
            var result = await _hostelService.GetHostelStatsAsync(hostelId);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Hostel not found"
                });
            }
            
            return Ok(new ApiResponse<HostelStatsDto>
            {
                Success = true,
                Message = "Hostel statistics retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting hostel stats: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve hostel statistics"
            });
        }
    }
    
    /// <summary>
    /// Update hostel (Admin only)
    /// </summary>
    [HttpPut("{hostelId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<HostelDto>>> UpdateHostelAsync(int hostelId, [FromBody] CreateHostelRequest request)
    {
        try
        {
            _logger.LogInformation($"Updating hostel: {hostelId}");
            
            var result = await _hostelService.UpdateHostelAsync(hostelId, request);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Hostel not found"
                });
            }
            
            return Ok(new ApiResponse<HostelDto>
            {
                Success = true,
                Message = "Hostel updated successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating hostel: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to update hostel"
            });
        }
    }
    
    /// <summary>
    /// Delete hostel (Admin only)
    /// </summary>
    [HttpDelete("{hostelId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteHostelAsync(int hostelId)
    {
        try
        {
            _logger.LogInformation($"Deleting hostel: {hostelId}");
            
            var result = await _hostelService.DeleteHostelAsync(hostelId);
            
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Hostel not found"
                });
            }
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Hostel deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting hostel: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to delete hostel"
            });
        }
    }
}
