using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Models.DTOs;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace SmartHostelManagementSystem.Services.Implementations;

/// <summary>
/// Authentication service implementation with JWT token generation
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(AppDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }
    
    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Validate email uniqueness
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }
            
            // Verify hostel exists if provided
            if (request.HostelId.HasValue)
            {
                var hostel = await _context.Hostels.FindAsync(request.HostelId.Value);
                if (hostel == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Hostel not found"
                    };
                }
            }
            else
            {
                // Default to first hostel for non-admin users
                if (request.Role != "Admin")
                {
                    var defaultHostel = await _context.Hostels.FirstOrDefaultAsync(h => !h.IsDeleted);
                    if (defaultHostel == null)
                    {
                        return new AuthResponse
                        {
                            Success = false,
                            Message = "No hostel available"
                        };
                    }
                    request.HostelId = defaultHostel.HostelId;
                }
                else
                {
                    request.HostelId = 1; // Default hostel for admin
                }
            }
            
            // Create new user
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = HashPassword(request.Password),
                Role = request.Role,
                HostelId = request.HostelId.Value,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Create student record if role is Student
            if (request.Role == "Student")
            {
                var student = new Student
                {
                    UserId = user.UserId,
                    HostelId = user.HostelId,
                    RollNumber = request.RollNumber,
                    AdmissionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }
            
            _logger.LogInformation($"User registered successfully: {user.Email}");
            
            return new AuthResponse
            {
                Success = true,
                Message = "User registered successfully",
                User = new UserDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    HostelId = user.HostelId,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Registration error: {ex.Message}");
            return new AuthResponse
            {
                Success = false,
                Message = $"Registration failed: {ex.Message}"
            };
        }
    }
    
    /// <summary>
    /// Login user and generate JWT token
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            
            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            
            // Generate JWT token
            var token = GenerateToken(user);
            
            _logger.LogInformation($"User logged in successfully: {user.Email}");
            
            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                User = new UserDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    HostelId = user.HostelId,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error: {ex.Message}");
            return new AuthResponse
            {
                Success = false,
                Message = $"Login failed: {ex.Message}"
            };
        }
    }
    
    /// <summary>
    /// Validate JWT token
    /// </summary>
    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "");
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Generate JWT token
    /// </summary>
    private string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "");
        
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim("UserId", user.UserId.ToString()),
            new System.Security.Claims.Claim("Email", user.Email),
            new System.Security.Claims.Claim("Role", user.Role),
            new System.Security.Claims.Claim("HostelId", user.HostelId.ToString())
        };
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(int.Parse(_configuration["Jwt:ExpiresInHours"] ?? "24")),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    /// <summary>
    /// Hash password using SHA256
    /// </summary>
    private static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
    
    /// <summary>
    /// Verify password against hash
    /// </summary>
    private static bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hash;
    }
}
