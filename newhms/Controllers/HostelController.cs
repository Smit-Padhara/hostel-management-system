using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// API Controller for hostel management operations
/// Endpoints for creating, reading, updating, and deleting hostels
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
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
    /// Create a new hostel
    /// POST: api/hostel
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(HostelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HostelDto>> CreateHostel([FromBody] CreateHostelRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hostel = await _hostelService.CreateHostelAsync(request);
            _logger.LogInformation($"Hostel '{hostel.Name}' created successfully with ID {hostel.HostelId}");
            
            return CreatedAtAction(nameof(GetHostelById), new { id = hostel.HostelId }, hostel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Failed to create hostel: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating hostel: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while creating the hostel" });
        }
    }

    /// <summary>
    /// Get all hostels
    /// GET: api/hostel
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HostelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HostelDto>>> GetAllHostels()
    {
        try
        {
            var hostels = await _hostelService.GetAllHostelsAsync();
            return Ok(hostels);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching hostels: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching hostels" });
        }
    }

    /// <summary>
    /// Get hostel by ID
    /// GET: api/hostel/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(HostelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HostelDto>> GetHostelById(int id)
    {
        try
        {
            var hostel = await _hostelService.GetHostelByIdAsync(id);
            
            if (hostel == null)
                return NotFound(new { message = $"Hostel with ID {id} not found" });

            return Ok(hostel);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching hostel {id}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching the hostel" });
        }
    }

    /// <summary>
    /// Get hostel with statistics
    /// GET: api/hostel/{id}/stats
    /// </summary>
    [HttpGet("{id}/stats")]
    [ProducesResponseType(typeof(HostelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HostelDto>> GetHostelStats(int id)
    {
        try
        {
            var hostel = await _hostelService.GetHostelWithStatsAsync(id);
            
            if (hostel == null)
                return NotFound(new { message = $"Hostel with ID {id} not found" });

            return Ok(hostel);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching hostel stats {id}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching hostel statistics" });
        }
    }

    /// <summary>
    /// Update hostel information
    /// PUT: api/hostel/{id}
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(HostelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HostelDto>> UpdateHostel(int id, [FromBody] CreateHostelRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hostel = await _hostelService.UpdateHostelAsync(id, request);
            
            if (hostel == null)
                return NotFound(new { message = $"Hostel with ID {id} not found" });

            _logger.LogInformation($"Hostel {id} updated successfully");
            return Ok(hostel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Failed to update hostel {id}: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating hostel {id}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the hostel" });
        }
    }

    /// <summary>
    /// Delete hostel (soft delete)
    /// DELETE: api/hostel/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHostel(int id)
    {
        try
        {
            var result = await _hostelService.DeleteHostelAsync(id);
            
            if (!result)
                return NotFound(new { message = $"Hostel with ID {id} not found" });

            _logger.LogInformation($"Hostel {id} deleted successfully");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting hostel {id}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the hostel" });
        }
    }
}
