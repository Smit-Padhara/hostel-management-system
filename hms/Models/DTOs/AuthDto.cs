using System.ComponentModel.DataAnnotations;

namespace SmartHostelManagementSystem.Models.DTOs;

/// <summary>
/// DTO for user registration
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = null!;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string PhoneNumber { get; set; } = null!;
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = null!;
    
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = null!; // "Admin", "Student", "Worker"
    
    public int? HostelId { get; set; }
    
    public string? RollNumber { get; set; } // For students
}

/// <summary>
/// DTO for user login
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = null!;
}

/// <summary>
/// DTO for authentication response
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public UserDto? User { get; set; }
}

/// <summary>
/// DTO for user information
/// </summary>
public class UserDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Role { get; set; } = null!;
    public int HostelId { get; set; }
    public DateTime CreatedAt { get; set; }
}
