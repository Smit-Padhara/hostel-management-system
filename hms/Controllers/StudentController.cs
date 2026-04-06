using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// Student Management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentController> _logger;
    
    public StudentController(IStudentService studentService, ILogger<StudentController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }
    
    private int GetHostelIdFromToken()
    {
        var hostelIdClaim = User.FindFirst("HostelId");
        return int.TryParse(hostelIdClaim?.Value, out var hostelId) ? hostelId : 0;
    }
    
    /// <summary>
    /// Create a new student (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> CreateStudentAsync([FromBody] CreateStudentRequest request)
    {
        try
        {
            _logger.LogInformation($"Creating student");
            
            var result = await _studentService.CreateStudentAsync(request);
            
            if (result == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create student"
                });
            }
            
            return Created(string.Empty, new ApiResponse<StudentDto>
            {
                Success = true,
                Message = "Student created successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating student: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to create student"
            });
        }
    }
    
    /// <summary>
    /// Get all students in a hostel (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<StudentDto>>>> GetAllStudentsAsync()
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching students for hostel: {hostelId}");
            
            var result = await _studentService.GetAllStudentsAsync(hostelId);
            
            return Ok(new ApiResponse<List<StudentDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} students",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting students: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve students"
            });
        }
    }
    
    /// <summary>
    /// Get student by ID
    /// </summary>
    [HttpGet("{studentId}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetStudentByIdAsync(int studentId)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching student: {studentId}");
            
            var result = await _studentService.GetStudentByIdAsync(studentId, hostelId);
            
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }
            
            return Ok(new ApiResponse<StudentDto>
            {
                Success = true,
                Message = "Student retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting student: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve student"
            });
        }
    }
    
    /// <summary>
    /// Allocate room to student (Admin only)
    /// </summary>
    [HttpPost("{studentId}/allocate-room")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> AllocateRoomAsync(int studentId, [FromBody] AllocateRoomRequest request)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Allocating room to student: {studentId}");
            
            var result = await _studentService.AllocateRoomAsync(hostelId, request);
            
            if (result == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to allocate room - either student/room not found or room is full"
                });
            }
            
            return Ok(new ApiResponse<StudentDto>
            {
                Success = true,
                Message = "Room allocated successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error allocating room: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to allocate room"
            });
        }
    }
    
    /// <summary>
    /// Get students in a specific room
    /// </summary>
    [HttpGet("room/{roomId}")]
    public async Task<ActionResult<ApiResponse<List<StudentDto>>>> GetStudentsByRoomAsync(int roomId)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Fetching students in room: {roomId}");
            
            var result = await _studentService.GetStudentsByRoomAsync(roomId, hostelId);
            
            return Ok(new ApiResponse<List<StudentDto>>
            {
                Success = true,
                Message = $"Retrieved {result.Count} students",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting students by room: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve students"
            });
        }
    }
    
    /// <summary>
    /// Remove student from hostel (Admin only)
    /// </summary>
    [HttpDelete("{studentId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveStudentAsync(int studentId)
    {
        try
        {
            var hostelId = GetHostelIdFromToken();
            _logger.LogInformation($"Removing student: {studentId}");
            
            var result = await _studentService.RemoveStudentAsync(studentId, hostelId);
            
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Student removed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error removing student: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to remove student"
            });
        }
    }
}
