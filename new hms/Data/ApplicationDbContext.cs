using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Models.Entities;

namespace SmartHostelManagementSystem.Data;

/// <summary>
/// Application DbContext with Identity support and multi-tenant configuration
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    
    // DbSets for all entities
    public DbSet<Hostel> Hostels => Set<Hostel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<Fee> Fees => Set<Fee>();
    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<CleaningRecord> CleaningRecords => Set<CleaningRecord>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // ============= HOSTEL CONFIGURATION =============
        modelBuilder.Entity<Hostel>(entity =>
        {
            entity.HasKey(e => e.HostelId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // Indexes for common queries
            entity.HasIndex(e => e.Name);
        });
        
        // ============= APPLICATION USER CONFIGURATION =============
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(256);
            
            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // Foreign key to Hostel
            entity.HasOne(e => e.Hostel)
                .WithMany(h => h.Users)
                .HasForeignKey(e => e.HostelId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // One-to-one relationships
            entity.HasOne(e => e.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Worker)
                .WithOne(w => w.User)
                .HasForeignKey<Worker>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.HostelId, e.Email }).IsUnique();
        });
        
        // ============= ROOM CONFIGURATION =============
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId);
            entity.Property(e => e.RoomNumber).IsRequired().HasMaxLength(50);
            
            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // Foreign key to Hostel
            entity.HasOne(e => e.Hostel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(e => e.HostelId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for multi-tenant queries
            entity.HasIndex(e => new { e.HostelId, e.RoomNumber }).IsUnique();
        });
        
        // ============= STUDENT CONFIGURATION =============
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId);
            entity.Property(e => e.RollNumber).IsRequired().HasMaxLength(50);
            
            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // Foreign keys
            entity.HasOne(e => e.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Room)
                .WithMany(r => r.Students)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Hostel)
                .WithMany(h => h.Students)
                .HasForeignKey(e => e.HostelId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.HostelId, e.RollNumber }).IsUnique();
        });
        
        // ============= COMPLAINT CONFIGURATION =============
        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(e => e.ComplaintId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            
            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // Foreign keys
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Complaints)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Hostel)
                .WithMany(h => h.Complaints)
                .HasForeignKey(e => e.HostelId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.HostelId, e.Status });
        });
        
        // ============= FEE CONFIGURATION =============
        modelBuilder.Entity<Fee>(entity =>
        {
            entity.HasKey(e => e.FeeId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            
            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // Foreign keys
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Fees)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Hostel)
                .WithMany(h => h.Fees)
                .HasForeignKey(e => e.HostelId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.HostelId, e.Status });
        });
        
        // ============= WORKER CONFIGURATION =============
        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.WorkerId);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
            
            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // Foreign keys
            entity.HasOne(e => e.User)
                .WithOne(u => u.Worker)
                .HasForeignKey<Worker>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Hostel)
                .WithMany(h => h.Workers)
                .HasForeignKey(e => e.HostelId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.HostelId, e.Department });
        });
        
        // ============= CLEANING RECORD CONFIGURATION =============
        modelBuilder.Entity<CleaningRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            
            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // Foreign keys
            entity.HasOne(e => e.Room)
                .WithMany(r => r.CleaningRecords)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Worker)
                .WithMany(w => w.CleaningRecords)
                .HasForeignKey(e => e.WorkerId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Composite index for daily reports
            entity.HasIndex(e => new { e.RoomId, e.Date });
        });
    }
}
