using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// API Controller for dashboard and analytics
/// Provides comprehensive hostel metrics and monitoring data
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive dashboard summary with all metrics
    /// GET: api/dashboard/{hostelId}
    /// </summary>
    [HttpGet("{hostelId}")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(int hostelId)
    {
        try
        {
            var summary = await _dashboardService.GetDashboardSummaryAsync(hostelId);
            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Hostel not found: {ex.Message}");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching dashboard summary: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching dashboard data" });
        }
    }

    /// <summary>
    /// Get recent complaints for dashboard display
    /// GET: api/dashboard/{hostelId}/recent-complaints?limit=5
    /// </summary>
    [HttpGet("{hostelId}/recent-complaints")]
    [ProducesResponseType(typeof(IEnumerable<ComplaintDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetRecentComplaints(
        int hostelId, 
        [FromQuery] int limit = 5)
    {
        try
        {
            var complaints = await _dashboardService.GetRecentComplaintsAsync(hostelId, limit);
            return Ok(complaints);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching recent complaints: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching recent complaints" });
        }
    }

    /// <summary>
    /// Get critical alerts for dashboard
    /// GET: api/dashboard/{hostelId}/alerts
    /// High priority issues: overdue fees, pending complaints, uncleaned rooms
    /// </summary>
    [HttpGet("{hostelId}/alerts")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, object>>> GetCriticalAlerts(int hostelId)
    {
        try
        {
            var alerts = await _dashboardService.GetCriticalAlertsAsync(hostelId);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching critical alerts: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching alerts" });
        }
    }

    /// <summary>
    /// Get hostel utilization metrics
    /// GET: api/dashboard/{hostelId}/utilization
    /// Returns room capacity, occupancy, and related metrics
    /// </summary>
    [HttpGet("{hostelId}/utilization")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, object>>> GetUtilizationMetrics(int hostelId)
    {
        try
        {
            var metrics = await _dashboardService.GetUtilizationMetricsAsync(hostelId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching utilization metrics: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching utilization metrics" });
        }
    }

    /// <summary>
    /// Get complaint resolution metrics
    /// GET: api/dashboard/{hostelId}/complaint-metrics
    /// Returns complaint counts by status and resolution rates
    /// </summary>
    [HttpGet("{hostelId}/complaint-metrics")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, object>>> GetComplaintMetrics(int hostelId)
    {
        try
        {
            var metrics = await _dashboardService.GetComplaintMetricsAsync(hostelId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching complaint metrics: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching complaint metrics" });
        }
    }

    /// <summary>
    /// Get fee collection metrics
    /// GET: api/dashboard/{hostelId}/fee-metrics
    /// Returns collection rates, pending/overdue amounts, and related data
    /// </summary>
    [HttpGet("{hostelId}/fee-metrics")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, object>>> GetFeeMetrics(int hostelId)
    {
        try
        {
            var metrics = await _dashboardService.GetFeeMetricsAsync(hostelId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching fee metrics: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching fee metrics" });
        }
    }

    /// <summary>
    /// Cache all dashboard data for improved performance
    /// POST: api/dashboard/{hostelId}/cache
    /// </summary>
    [HttpPost("{hostelId}/cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CacheDashboardData(int hostelId)
    {
        try
        {
            await _dashboardService.CacheDashboardDataAsync(hostelId);
            return Ok(new { message = "Dashboard data cached successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error caching dashboard data: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while caching dashboard data" });
        }
    }

    /// <summary>
    /// Invalidate dashboard cache
    /// DELETE: api/dashboard/{hostelId}/cache
    /// </summary>
    [HttpDelete("{hostelId}/cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> InvalidateCache(int hostelId)
    {
        try
        {
            await _dashboardService.InvalidateDashboardCacheAsync(hostelId);
            return Ok(new { message = "Dashboard cache invalidated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error invalidating dashboard cache: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while invalidating cache" });
        }
    }

    /// <summary>
    /// Health check endpoint - not hostel specific
    /// GET: api/dashboard/health
    /// </summary>
    [HttpGet("health/check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
