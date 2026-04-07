# 🎯 API Design & Implementation Guidelines

## Overview

This guide provides standards for implementing REST API endpoints for the Smart Hostel Management System backend.

---

## API Design Principles

### 1. RESTful Conventions
```
GET    /api/students              - Get all students
GET    /api/students/{id}         - Get student by ID
POST   /api/students              - Create new student
PUT    /api/students/{id}         - Update student
DELETE /api/students/{id}         - Delete student (soft delete)
PATCH  /api/students/{id}         - Partial update
```

### 2. Request/Response Patterns

**Standard Response Format:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "John Doe",
    ...
  },
  "message": "Operation completed successfully"
}
```

**Error Response Format:**
```json
{
  "success": false,
  "error": "Student not found",
  "errors": ["Detailed error information"],
  "statusCode": 404
}
```

### 3. HTTP Status Codes

| Code | Usage |
|------|-------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 204 | No Content - Successful, no response body |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource conflict |
| 500 | Server Error - Internal error |

### 4. Pagination

```csharp
// Query Parameters
GET /api/students?pageNumber=1&pageSize=10&sortBy=name&sortOrder=asc

// Response
{
  "data": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 45,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 5. Filtering

```csharp
// Query Parameters
GET /api/students?hostelId=1&search=John&status=active

// Example: Get students in hostel 1 with name containing "John"
GET /api/students?hostelId=1&search=John
```

---

## Request DTOs

### Create DTO Pattern

```csharp
using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.Models.DTOs.Requests;

/// <summary>
/// DTO for creating a new student
/// </summary>
public class CreateStudentDto
{
    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Hostel ID is required")]
    public int HostelId { get; set; }

    [Required(ErrorMessage = "Roll number is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Roll number must be between 3-50 characters")]
    public string RollNumber { get; set; } = string.Empty;

    public int? RoomId { get; set; }

    [DataType(DataType.Date)]
    public DateTime AdmissionDate { get; set; }
}
```

### Update DTO Pattern

```csharp
namespace SmartHostelManagementSystem.Models.DTOs.Requests;

/// <summary>
/// DTO for updating a student
/// </summary>
public class UpdateStudentDto
{
    [StringLength(50)]
    public string? RollNumber { get; set; }

    public int? RoomId { get; set; }

    [DataType(DataType.Date)]
    public DateTime? AdmissionDate { get; set; }
}
```

---

## Response DTOs

```csharp
namespace SmartHostelManagementSystem.Models.DTOs.Responses;

/// <summary>
/// DTO for student response
/// </summary>
public class StudentResponseDto
{
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public int HostelId { get; set; }
    public int? RoomId { get; set; }
    public string RollNumber { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Related data
    public UserResponseDto? User { get; set; }
    public RoomResponseDto? Room { get; set; }
}

public class UserResponseDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class RoomResponseDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
}
```

---

## Service Layer Pattern

```csharp
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Models.DTOs.Requests;
using SmartHostelManagementSystem.Models.DTOs.Responses;
using SmartHostelManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Service for managing students
/// </summary>
public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentService> _logger;
    private readonly ICacheService _cacheService;

    public StudentService(
        ApplicationDbContext context,
        ILogger<StudentService> logger,
        ICacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Get all students for a hostel
    /// </summary>
    public async Task<List<StudentResponseDto>> GetStudentsAsync(int hostelId, int pageNumber = 1, int pageSize = 10)
    {
        _logger.LogInformation("Fetching students for hostel {HostelId}, page {PageNumber}", hostelId, pageNumber);

        var students = await _context.Students
            .Where(s => s.HostelId == hostelId)
            .Include(s => s.User)
            .Include(s => s.Room)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentResponseDto
            {
                StudentId = s.StudentId,
                UserId = s.UserId,
                HostelId = s.HostelId,
                RoomId = s.RoomId,
                RollNumber = s.RollNumber,
                AdmissionDate = s.AdmissionDate,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                User = new UserResponseDto
                {
                    Id = s.User!.Id,
                    UserName = s.User.UserName!,
                    Email = s.User.Email!,
                    FullName = s.User.FullName
                },
                Room = s.Room != null ? new RoomResponseDto
                {
                    RoomId = s.Room.RoomId,
                    RoomNumber = s.Room.RoomNumber,
                    Capacity = s.Room.Capacity,
                    CurrentOccupancy = s.Room.CurrentOccupancy
                } : null
            })
            .ToListAsync();

        return students;
    }

    /// <summary>
    /// Get student by ID
    /// </summary>
    public async Task<StudentResponseDto> GetStudentByIdAsync(int studentId, int hostelId)
    {
        _logger.LogInformation("Fetching student {StudentId} for hostel {HostelId}", studentId, hostelId);

        // Try cache first
        var cacheKey = $"student:{studentId}";
        var cachedStudent = await _cacheService.GetAsync<StudentResponseDto>(cacheKey);
        if (cachedStudent != null)
        {
            _logger.LogDebug("Student {StudentId} found in cache", studentId);
            return cachedStudent;
        }

        var student = await _context.Students
            .Where(s => s.StudentId == studentId && s.HostelId == hostelId)
            .Include(s => s.User)
            .Include(s => s.Room)
            .FirstOrDefaultAsync();

        if (student == null)
        {
            _logger.LogWarning("Student {StudentId} not found", studentId);
            throw new KeyNotFoundException($"Student with ID {studentId} not found");
        }

        var response = new StudentResponseDto
        {
            StudentId = student.StudentId,
            UserId = student.UserId,
            HostelId = student.HostelId,
            RoomId = student.RoomId,
            RollNumber = student.RollNumber,
            AdmissionDate = student.AdmissionDate,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt,
            User = new UserResponseDto
            {
                Id = student.User!.Id,
                UserName = student.User.UserName!,
                Email = student.User.Email!,
                FullName = student.User.FullName
            },
            Room = student.Room != null ? new RoomResponseDto
            {
                RoomId = student.Room.RoomId,
                RoomNumber = student.Room.RoomNumber,
                Capacity = student.Room.Capacity,
                CurrentOccupancy = student.Room.CurrentOccupancy
            } : null
        };

        // Cache the result
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(1));

        return response;
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    public async Task<StudentResponseDto> CreateStudentAsync(int hostelId, CreateStudentDto dto)
    {
        _logger.LogInformation("Creating student for hostel {HostelId}", hostelId);

        // Validation
        var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId && u.HostelId == hostelId);
        if (!userExists)
            throw new ArgumentException("User not found in this hostel");

        var rollNumberExists = await _context.Students
            .AnyAsync(s => s.RollNumber == dto.RollNumber && s.HostelId == hostelId && !s.IsDeleted);
        if (rollNumberExists)
            throw new ArgumentException("Roll number already exists in this hostel");

        // Create entity
        var student = new Student
        {
            UserId = dto.UserId,
            HostelId = hostelId,
            RoomId = dto.RoomId,
            RollNumber = dto.RollNumber,
            AdmissionDate = dto.AdmissionDate
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Student {StudentId} created successfully", student.StudentId);

        // Invalidate cache
        await _cacheService.RemoveAsync($"students:hostel:{hostelId}");

        return await GetStudentByIdAsync(student.StudentId, hostelId);
    }

    /// <summary>
    /// Update a student
    /// </summary>
    public async Task<StudentResponseDto> UpdateStudentAsync(int studentId, int hostelId, UpdateStudentDto dto)
    {
        _logger.LogInformation("Updating student {StudentId}", studentId);

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == studentId && s.HostelId == hostelId);

        if (student == null)
            throw new KeyNotFoundException($"Student with ID {studentId} not found");

        // Update properties
        if (!string.IsNullOrEmpty(dto.RollNumber))
        {
            var exists = await _context.Students
                .AnyAsync(s => s.RollNumber == dto.RollNumber && s.StudentId != studentId && s.HostelId == hostelId && !s.IsDeleted);
            if (exists)
                throw new ArgumentException("Roll number already exists");

            student.RollNumber = dto.RollNumber;
        }

        if (dto.RoomId.HasValue)
            student.RoomId = dto.RoomId;

        if (dto.AdmissionDate.HasValue)
            student.AdmissionDate = dto.AdmissionDate.Value;

        student.UpdatedAt = DateTime.UtcNow;

        _context.Students.Update(student);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Student {StudentId} updated successfully", studentId);

        // Invalidate cache
        await _cacheService.RemoveAsync($"student:{studentId}");

        return await GetStudentByIdAsync(studentId, hostelId);
    }

    /// <summary>
    /// Delete a student (soft delete)
    /// </summary>
    public async Task DeleteStudentAsync(int studentId, int hostelId)
    {
        _logger.LogInformation("Deleting student {StudentId}", studentId);

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == studentId && s.HostelId == hostelId);

        if (student == null)
            throw new KeyNotFoundException($"Student with ID {studentId} not found");

        student.IsDeleted = true;
        student.UpdatedAt = DateTime.UtcNow;

        _context.Students.Update(student);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Student {StudentId} deleted successfully", studentId);

        // Invalidate cache
        await _cacheService.RemoveAsync($"student:{studentId}");
    }
}
```

---

## Service Interface Pattern

```csharp
using SmartHostelManagementSystem.Models.DTOs.Requests;
using SmartHostelManagementSystem.Models.DTOs.Responses;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for student service
/// </summary>
public interface IStudentService
{
    /// <summary>
    /// Get all students for a hostel with pagination
    /// </summary>
    Task<List<StudentResponseDto>> GetStudentsAsync(int hostelId, int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Get student by ID
    /// </summary>
    Task<StudentResponseDto> GetStudentByIdAsync(int studentId, int hostelId);

    /// <summary>
    /// Create new student
    /// </summary>
    Task<StudentResponseDto> CreateStudentAsync(int hostelId, CreateStudentDto dto);

    /// <summary>
    /// Update student
    /// </summary>
    Task<StudentResponseDto> UpdateStudentAsync(int studentId, int hostelId, UpdateStudentDto dto);

    /// <summary>
    /// Delete student (soft delete)
    /// </summary>
    Task DeleteStudentAsync(int studentId, int hostelId);
}
```

---

## Controller Pattern

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.Models.DTOs.Requests;
using SmartHostelManagementSystem.Services.Interfaces;
using System.Security.Claims;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// Students API controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all students for a hostel
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StudentResponseDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetStudents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var hostelId = int.Parse(User.FindFirst("hostelId")?.Value ?? "0");
            if (hostelId == 0)
                return Unauthorized("Hostel information not found in token");

            var students = await _studentService.GetStudentsAsync(hostelId, pageNumber, pageSize);
            
            return Ok(new
            {
                success = true,
                data = students,
                message = $"Retrieved {students.Count} students"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get student by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StudentResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetStudent(int id)
    {
        try
        {
            var hostelId = int.Parse(User.FindFirst("hostelId")?.Value ?? "0");
            if (hostelId == 0)
                return Unauthorized("Hostel information not found in token");

            var student = await _studentService.GetStudentByIdAsync(id, hostelId);
            
            return Ok(new
            {
                success = true,
                data = student
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Student not found: {Id}", id);
            return NotFound(new { success = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Create new student (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(StudentResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hostelId = int.Parse(User.FindFirst("hostelId")?.Value ?? "0");
            if (hostelId == 0)
                return Unauthorized("Hostel information not found in token");

            var student = await _studentService.CreateStudentAsync(hostelId, dto);
            
            return CreatedAtAction(nameof(GetStudent), new { id = student.StudentId }, new
            {
                success = true,
                data = student,
                message = "Student created successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
            return BadRequest(new { success = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Update student (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(StudentResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hostelId = int.Parse(User.FindFirst("hostelId")?.Value ?? "0");
            if (hostelId == 0)
                return Unauthorized("Hostel information not found in token");

            var student = await _studentService.UpdateStudentAsync(id, hostelId, dto);
            
            return Ok(new
            {
                success = true,
                data = student,
                message = "Student updated successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Student not found: {Id}", id);
            return NotFound(new { success = false, error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
            return BadRequest(new { success = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Delete student (Admin only) - Soft delete
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        try
        {
            var hostelId = int.Parse(User.FindFirst("hostelId")?.Value ?? "0");
            if (hostelId == 0)
                return Unauthorized("Hostel information not found in token");

            await _studentService.DeleteStudentAsync(id, hostelId);
            
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Student not found: {Id}", id);
            return NotFound(new { success = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }
}
```

---

## Key Principles

### 1. Multi-Tenant Awareness
- Always extract `hostelId` from JWT claims
- Filter queries by hostelId
- Prevent cross-tenant data access

### 2. Exception Handling
- Catch specific exceptions first
- Log with appropriate levels
- Return meaningful error messages

### 3. Caching Strategy
- Cache frequently accessed data
- Invalidate on write operations
- Use consistent cache keys

### 4. Validation
- Validate DTOs in controllers
- Validate business rules in services
- Return 400 for validation errors

### 5. Async Operations
- Use async/await throughout
- Non-blocking I/O operations
- Proper task composition

### 6. Logging
- Log operations at appropriate levels
- Include context (IDs, hostels)
- Never log sensitive data

### 7. Documentation
- Add XML comments to all public members
- Document parameters and return values
- Include example usage

---

## Standard DTO Locations

```
Models/DTOs/
├── Requests/
│   ├── CreateStudentDto.cs
│   ├── UpdateStudentDto.cs
│   ├── CreateComplaintDto.cs
│   ├── UpdateComplaintDto.cs
│   └── ...
└── Responses/
    ├── StudentResponseDto.cs
    ├── ComplaintResponseDto.cs
    ├── PagedResponseDto.cs
    └── ...
```

---

## Service Registration (Program.cs)

```csharp
// Add DTOs namespace mapping
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IFeeService, FeeService>();
// ... add all services

builder.Services.AddControllers();
```

---

## Testing the API

### Using Swagger
1. Open `https://localhost:5001/swagger`
2. Click "Authorize"
3. Enter JWT token
4. Test endpoints

### Using curl

```bash
# Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@hms.com","password":"Admin@123"}'

# Get students (with token)
curl -X GET https://localhost:5001/api/students \
  -H "Authorization: Bearer {token}"

# Create student
curl -X POST https://localhost:5001/api/students \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"userId":2,"hostelId":1,"rollNumber":"2024001"}'
```

---

This guide ensures consistency and maintainability across all API implementations.
