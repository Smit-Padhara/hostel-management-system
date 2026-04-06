using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Controllers;

/// <summary>
/// Authentication API endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserDto>>> RegisterAsync([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation($"Registration request for email: {request.Email}");
            
            var result = await _authService.RegisterAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }
            
            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = result.Message,
                Data = result.User
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Registration error: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Registration failed"
            });
        }
    }
    
    /// <summary>
    /// Login user and get JWT token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> LoginAsync([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Login request for email: {request.Email}");
            
            var result = await _authService.LoginAsync(request);
            
            if (!result.Success)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = result.Message,
                Data = new
                {
                    token = result.Token,
                    user = result.User
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error: {ex.Message}");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Login failed"
            });
        }
    }
    
    /// <summary>
    /// Hello World endpoint for testing
    /// </summary>
    [HttpGet("hello")]
    [AllowAnonymous]
    public IActionResult HelloWorld()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Hello from Smart Hostel Management System API v1!",
            Data = $"Current time: {DateTime.UtcNow:O}"
        });
    }
}
