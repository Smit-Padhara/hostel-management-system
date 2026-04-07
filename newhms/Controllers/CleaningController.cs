using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// API Controller for cleaning management operations (CORE MODULE)
/// Handles cleaning task tracking, room status updates, and cleaning reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CleaningController : ControllerBase
{
    private readonly ICleaningService _cleaningService;
    private readonly ILogger<CleaningController> _logger;

    public CleaningController(ICleaningService cleaningService, ILogger<CleaningController> logger)
    {
        _cleaningService = cleaningService;
        _logger = logger;
    }

    /// <summary>
    /// Create a cleaning record for a room
    /// POST: api/cleaning
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CleaningRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CleaningRecordDto>> CreateCleaningRecord(
        [FromQuery] int roomId, 
        [FromQuery] DateTime? date = null)
    {
        try
        {
            var recordDate = date ?? DateTime.UtcNow;
            var record = await _cleaningService.CreateCleaningRecordAsync(roomId, recordDate);
            _logger.LogInformation($"Cleaning record created for room {roomId}");
            
            return CreatedAtAction(nameof(GetCleaningRecordById), new { id = record.RecordId }, record);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Failed to create cleaning record: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating cleaning record: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while creating the cleaning record" });
        }
    }

    /// <summary>
    /// Get cleaning record by ID
    /// GET: api/cleaning/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CleaningRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CleaningRecordDto>> GetCleaningRecordById(int id)
    {
        try
        {
            var record = await _cleaningService.GetCleaningRecordByIdAsync(id);
            
            if (record == null)
                return NotFound(new { message = $"Cleaning record with ID {id} not found" });

            return Ok(record);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching cleaning record: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching the cleaning record" });
        }
    }

    /// <summary>
    /// Get cleaning records for a room
    /// GET: api/cleaning/room/{roomId}
    /// </summary>
    [HttpGet("room/{roomId}")]
    [ProducesResponseType(typeof(IEnumerable<CleaningRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CleaningRecordDto>>> GetRecordsByRoom(int roomId)
    {
        try
        {
            var records = await _cleaningService.GetCleaningRecordsByRoomAsync(roomId);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching cleaning records for room: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching cleaning records" });
        }
    }

    /// <summary>
    /// Get today's cleaning tasks for a hostel (CORE REQUIREMENT)
    /// GET: api/cleaning/today/{hostelId}
    /// </summary>
    [HttpGet("today/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<CleaningRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CleaningRecordDto>>> GetTodaysTasks(int hostelId)
    {
        try
        {
            var tasks = await _cleaningService.GetTodaysCleaningTasksAsync(hostelId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching today's cleaning tasks: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching cleaning tasks" });
        }
    }

    /// <summary>
    /// Get pending cleaning records (CORE REQUIREMENT)
    /// GET: api/cleaning/pending/{hostelId}
    /// </summary>
    [HttpGet("pending/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<CleaningRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CleaningRecordDto>>> GetPendingRecords(int hostelId)
    {
        try
        {
            var records = await _cleaningService.GetPendingCleaningRecordsAsync(hostelId);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching pending cleaning records: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching pending records" });
        }
    }

    /// <summary>
    /// Get pending cleaning count (CORE REQUIREMENT)
    /// GET: api/cleaning/pending-count/{hostelId}
    /// </summary>
    [HttpGet("pending-count/{hostelId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPendingCount(int hostelId)
    {
        try
        {
            var count = await _cleaningService.GetPendingCleaningCountAsync(hostelId);
            return Ok(new { hostelId = hostelId, pendingRooms = count });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting pending cleaning count: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Update cleaning record status
    /// PUT: api/cleaning/{id}/status
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(CleaningRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CleaningRecordDto>> UpdateCleaningStatus(
        int id, 
        [FromBody] UpdateCleaningStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = await _cleaningService.UpdateCleaningStatusAsync(id, request);
            
            if (record == null)
                return NotFound(new { message = $"Cleaning record with ID {id} not found" });

            _logger.LogInformation($"Cleaning record {id} status updated to {request.Status}");
            return Ok(record);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating cleaning status: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the cleaning record" });
        }
    }

    /// <summary>
    /// Mark room as cleaned - Convenience endpoint
    /// POST: api/cleaning/mark-cleaned/{roomId}
    /// </summary>
    [HttpPost("mark-cleaned/{roomId}")]
    [ProducesResponseType(typeof(CleaningRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CleaningRecordDto>> MarkRoomCleaned(
        int roomId, 
        [FromQuery] int? workerId = null,
        [FromQuery] string? remarks = null)
    {
        try
        {
            var record = await _cleaningService.MarkRoomAsCleanedAsync(roomId, workerId, remarks);
            
            if (record == null)
                return NotFound(new { message = $"No cleaning record found for room {roomId} today" });

            _logger.LogInformation($"Room {roomId} marked as cleaned");
            return Ok(record);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking room as cleaned: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get cleaning history for a room (CORE REQUIREMENT)
    /// GET: api/cleaning/history/{roomId}?days=30
    /// </summary>
    [HttpGet("history/{roomId}")]
    [ProducesResponseType(typeof(IEnumerable<CleaningRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CleaningRecordDto>>> GetCleaningHistory(
        int roomId, 
        [FromQuery] int days = 30)
    {
        try
        {
            var history = await _cleaningService.GetCleaningHistoryAsync(roomId, days);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching cleaning history: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching cleaning history" });
        }
    }

    /// <summary>
    /// Get cleaning report for a hostel (CORE REQUIREMENT)
    /// GET: api/cleaning/report/{hostelId}?date=2024-01-15
    /// </summary>
    [HttpGet("report/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<CleaningRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CleaningRecordDto>>> GetCleaningReport(
        int hostelId, 
        [FromQuery] DateTime? date = null)
    {
        try
        {
            var reportDate = date ?? DateTime.UtcNow;
            var report = await _cleaningService.GetCleaningReportAsync(hostelId, reportDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching cleaning report: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching cleaning report" });
        }
    }

    /// <summary>
    /// Get uncleaned rooms (CORE REQUIREMENT)
    /// GET: api/cleaning/uncleaned/{hostelId}?days=3
    /// </summary>
    [HttpGet("uncleaned/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<CleaningRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CleaningRecordDto>>> GetUncleanlRooms(
        int hostelId, 
        [FromQuery] int days = 3)
    {
        try
        {
            var uncleanlRooms = await _cleaningService.GetUncleanlRoomsAsync(hostelId, days);
            return Ok(uncleanlRooms);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching uncleaned rooms: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching uncleaned rooms" });
        }
    }

    /// <summary>
    /// Delete cleaning record (soft delete)
    /// DELETE: api/cleaning/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCleaningRecord(int id)
    {
        try
        {
            var result = await _cleaningService.DeleteCleaningRecordAsync(id);
            
            if (!result)
                return NotFound(new { message = $"Cleaning record with ID {id} not found" });

            _logger.LogInformation($"Cleaning record {id} deleted successfully");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting cleaning record: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the record" });
        }
    }
}
