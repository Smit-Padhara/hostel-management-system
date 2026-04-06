using SmartHostelManagementSystem.Models.DTOs;

namespace SmartHostelManagementSystem.Services.Interfaces;

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    
    /// <summary>
    /// Login user and generate JWT token
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Validate JWT token
    /// </summary>
    bool ValidateToken(string token);
}
