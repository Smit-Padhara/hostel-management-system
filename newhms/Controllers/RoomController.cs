using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// API Controller for room management operations
/// Handles room operations with capacity checking and over-allocation prevention
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomController> _logger;

    public RoomController(IRoomService roomService, ILogger<RoomController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new room
    /// POST: api/room
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var room = await _roomService.CreateRoomAsync(request);
            _logger.LogInformation($"Room '{room.RoomNumber}' created in hostel {request.HostelId}");
            
            return CreatedAtAction(nameof(GetRoomById), new { id = room.RoomId }, room);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Failed to create room: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating room: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while creating the room" });
        }
    }

    /// <summary>
    /// Get room by ID
    /// GET: api/room/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomDto>> GetRoomById(int id)
    {
        try
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            
            if (room == null)
                return NotFound(new { message = $"Room with ID {id} not found" });

            return Ok(room);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching room {id}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching the room" });
        }
    }

    /// <summary>
    /// Get all rooms in a hostel
    /// GET: api/room/hostel/{hostelId}
    /// </summary>
    [HttpGet("hostel/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRoomsByHostel(int hostelId)
    {
        try
        {
            var rooms = await _roomService.GetRoomsByHostelAsync(hostelId);
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching rooms for hostel {hostelId}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching rooms" });
        }
    }

    /// <summary>
    /// Get available rooms (with free capacity) - IMPORTANT Business Rule
    /// GET: api/room/available/{hostelId}
    /// </summary>
    [HttpGet("available/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAvailableRooms(int hostelId)
    {
        try
        {
            var availableRooms = await _roomService.GetAvailableRoomsAsync(hostelId);
            return Ok(availableRooms);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching available rooms: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching available rooms" });
        }
    }

    /// <summary>
    /// Check room capacity (available seats)
    /// GET: api/room/{id}/capacity
    /// </summary>
    [HttpGet("{id}/capacity")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CheckCapacity(int id)
    {
        try
        {
            var availableSeats = await _roomService.CheckRoomCapacityAsync(id);
            return Ok(new { roomId = id, availableSeats = availableSeats });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking room capacity: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while checking room capacity" });
        }
    }

    /// <summary>
    /// Check if room is full
    /// GET: api/room/{id}/full
    /// </summary>
    [HttpGet("{id}/full")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> IsRoomFull(int id)
    {
        try
        {
            var isFull = await _roomService.IsRoomFullAsync(id);
            return Ok(new { roomId = id, isFull = isFull });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking if room is full: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Update room information
    /// PUT: api/room/{id}
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomDto>> UpdateRoom(int id, [FromBody] CreateRoomRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var room = await _roomService.UpdateRoomAsync(id, request);
            
            if (room == null)
                return NotFound(new { message = $"Room with ID {id} not found" });

            _logger.LogInformation($"Room {id} updated successfully");
            return Ok(room);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Failed to update room: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating room: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the room" });
        }
    }

    /// <summary>
    /// Delete room (soft delete)
    /// DELETE: api/room/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        try
        {
            var result = await _roomService.DeleteRoomAsync(id);
            
            if (!result)
                return NotFound(new { message = $"Room with ID {id} not found" });

            _logger.LogInformation($"Room {id} deleted successfully");
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Failed to delete room: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting room: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the room" });
        }
    }
}
