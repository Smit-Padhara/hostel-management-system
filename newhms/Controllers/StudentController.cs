using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.DTOs.Requests;
using SmartHostelManagementSystem.DTOs.Responses;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// API Controller for student management operations
/// Handles student assignment with capacity validation and over-allocation prevention
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentController> _logger;

    public StudentController(IStudentService studentService, ILogger<StudentController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    /// <summary>
    /// Get student by ID
    /// GET: api/student/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetStudentById(int id)
    {
        try
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            
            if (student == null)
                return NotFound(new { message = $"Student with ID {id} not found" });

            return Ok(student);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching student {id}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching the student" });
        }
    }

    /// <summary>
    /// Get all students in a hostel
    /// GET: api/student/hostel/{hostelId}
    /// </summary>
    [HttpGet("hostel/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsByHostel(int hostelId)
    {
        try
        {
            var students = await _studentService.GetStudentsByHostelAsync(hostelId);
            return Ok(students);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching students for hostel {hostelId}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching students" });
        }
    }

    /// <summary>
    /// Get all students in a specific room
    /// GET: api/student/room/{roomId}
    /// </summary>
    [HttpGet("room/{roomId}")]
    [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsByRoom(int roomId)
    {
        try
        {
            var students = await _studentService.GetStudentsByRoomAsync(roomId);
            return Ok(students);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching students in room {roomId}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching students" });
        }
    }

    /// <summary>
    /// Get unassigned students in a hostel
    /// GET: api/student/unassigned/{hostelId}
    /// </summary>
    [HttpGet("unassigned/{hostelId}")]
    [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetUnassignedStudents(int hostelId)
    {
        try
        {
            var students = await _studentService.GetUnassignedStudentsAsync(hostelId);
            return Ok(students);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching unassigned students: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while fetching unassigned students" });
        }
    }

    /// <summary>
    /// Assign student to room with capacity validation
    /// POST: api/student/assign
    /// IMPORTANT: Prevents room over-allocation
    /// </summary>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StudentDto>> AssignStudentToRoom([FromBody] AssignStudentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var student = await _studentService.AssignStudentToRoomAsync(request);
            _logger.LogInformation($"Student {request.StudentId} assigned to room {request.RoomId}");
            
            return Ok(student);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Failed to assign student: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error assigning student: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while assigning the student" });
        }
    }

    /// <summary>
    /// Unassign student from their room
    /// POST: api/student/{id}/unassign
    /// </summary>
    [HttpPost("{id}/unassign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UnassignStudent(int id)
    {
        try
        {
            var result = await _studentService.UnassignStudentAsync(id);
            
            if (!result)
                return NotFound(new { message = $"Student with ID {id} not found" });

            _logger.LogInformation($"Student {id} unassigned successfully");
            return Ok(new { message = "Student unassigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error unassigning student: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while unassigning the student" });
        }
    }

    /// <summary>
    /// Get total student count in a hostel
    /// GET: api/student/count/{hostelId}
    /// </summary>
    [HttpGet("count/{hostelId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetStudentCount(int hostelId)
    {
        try
        {
            var count = await _studentService.GetStudentCountAsync(hostelId);
            return Ok(new { hostelId = hostelId, totalStudents = count });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting student count: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while getting student count" });
        }
    }

    /// <summary>
    /// Check if student is assigned
    /// GET: api/student/{id}/assigned
    /// </summary>
    [HttpGet("{id}/assigned")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> IsStudentAssigned(int id)
    {
        try
        {
            var isAssigned = await _studentService.IsStudentAssignedAsync(id);
            return Ok(new { studentId = id, isAssigned = isAssigned });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking student assignment: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred" });
        }
    }
}
