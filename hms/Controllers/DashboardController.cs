using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// Dashboard API endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;
    
    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }
    
    private int GetHostelIdFromToken()
    {
        var hostelIdClaim = User.FindFirst("HostelId");
        return int.TryParse(hostelIdClaim?.Value, out var hostelId) ? hostelId : 0;
    }
    
    /// <summary>
    /// Get admin dashboard statistics (Admin only)
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetAdminDashboardAsync()
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching admin dashboard for hostel: {hostelId}");
            
            var result = await _dashboardService.GetAdminDashboardAsync(hostelId);
            
            return Ok(new ApiResponse<AdminDashboardDto>
            {
                Success = true,
                Message = "Dashboard retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting admin dashboard: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve dashboard"
            });
        }
    }
    
    /// <summary>
    /// Get hostel quick statistics
    /// </summary>
    [HttpGet("hostel/stats")]
    public async Task<ActionResult<ApiResponse<HostelStatsDto>>> GetHostelQuickStatsAsync()
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching hostel stats for: {hostelId}");
            
            var result = await _dashboardService.GetHostelQuickStatsAsync(hostelId);
            
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
                Message = "Statistics retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting hostel stats: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve statistics"
            });
        }
    }
}
