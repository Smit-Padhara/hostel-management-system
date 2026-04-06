namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Represents a user in the system (Admin, Student, Worker)
/// </summary>
public class User
{
    public int UserId { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string Email { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;
    
    public string PasswordHash { get; set; } = null!;
    
    public string Role { get; set; } = null!; // "Admin", "Student", "Worker"
    
    public int HostelId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public Hostel? Hostel { get; set; }
    public Student? Student { get; set; }
    public Worker? Worker { get; set; }
}
