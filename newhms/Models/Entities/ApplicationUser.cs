using Microsoft.AspNetCore.Identity;

namespace SmartHostelManagementSystem.Models.Entities;

/// <summary>
/// Application user entity extending IdentityUser for authentication
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Foreign key to Hostel for multi-tenant support
    /// </summary>
    public int HostelId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public virtual Hostel? Hostel { get; set; }
    public virtual Student? Student { get; set; }
    public virtual Worker? Worker { get; set; }
}
