using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Services.Interfaces;
using System.Security.Claims;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// Room Management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomController> _logger;
    
    public RoomController(IRoomService roomService, ILogger<RoomController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }
    
    private int GetHostelIdFromToken()
    {
        var hostelIdClaim = User.FindFirst("HostelId");
        return int.TryParse(hostelIdClaim?.Value, out var hostelId) ? hostelId : 0;
    }
    
    /// <summary>
    /// Create a new room (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> CreateRoomAsync([FromBody] CreateRoomRequest request)
    {
        try
        {
            _logger.LogInformation($"Creating room: {request.RoomNumber}");
            
            var result = await _roomService.CreateRoomAsync(request);
            
            if (result == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create room"
                });
            }
            
            return Created(string.Empty, new ApiResponse<RoomDto>
            {
                Success = true,
                Message = "Room created successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating room: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to create room"
            });
        }
    }
    
    /// <summary>
    /// Get all rooms for a hostel
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoomDto>>>> GetAllRoomsAsync([FromQuery] int? hostelId = null)
    {
        try
        {
            var hostelIdToUse = hostelId ?? GetHostelIdFromToken();
            _logger.LogInformation($"Fetching rooms for hostel: {hostelIdToUse}");
            
            var result = await _roomService.GetAllRoomsAsync(hostelIdToUse);
            
            return Ok(new ApiResponse<List<RoomDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} rooms",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting rooms: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve rooms"
            });
        }
    }
    
    /// <summary>
    /// Get room by ID
    /// </summary>
    [HttpGet("{roomId}")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> GetRoomByIdAsync(int roomId)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching room: {roomId}");
            
            var result = await _roomService.GetRoomByIdAsync(roomId, hostelId);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Room not found"
                });
            }
            
            return Ok(new ApiResponse<RoomDto>
            {
                Success = true,
                Message = "Room retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting room: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve room"
            });
        }
    }
    
    /// <summary>
    /// Get available rooms
    /// </summary>
    [HttpGet("available/list")]
    public async Task<ActionResult<ApiResponse<List<RoomAvailabilityDto>>>> GetAvailableRoomsAsync()
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching available rooms for hostel: {hostelId}");
            
            var result = await _roomService.GetAvailableRoomsAsync(hostelId);
            
            return Ok(new ApiResponse<List<RoomAvailabilityDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} available rooms",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting available rooms: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve available rooms"
            });
        }
    }
    
    /// <summary>
    /// Update room (Admin only)
    /// </summary>
    [HttpPut("{roomId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> UpdateRoomAsync(int roomId, [FromBody] CreateRoomRequest request)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Updating room: {roomId}");
            
            var result = await _roomService.UpdateRoomAsync(roomId, hostelId, request);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Room not found"
                });
            }
            
            return Ok(new ApiResponse<RoomDto>
            {
                Success = true,
                Message = "Room updated successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating room: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to update room"
            });
        }
    }
    
    /// <summary>
    /// Delete room (Admin only)
    /// </summary>
    [HttpDelete("{roomId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRoomAsync(int roomId)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Deleting room: {roomId}");
            
            var result = await _roomService.DeleteRoomAsync(roomId, hostelId);
            
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Room not found"
                });
            }
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Room deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting room: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to delete room"
            });
        }
    }
}
