using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Models.Entities;

namespace SmartHostelManagementSystem.Data;

/// <summary>
/// Application database context for Smart Hostel Management System
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    // DbSets
    public DbSet<Hostel> Hostels { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Complaint> Complaints { get; set; } = null!;
    public DbSet<Fee> Fees { get; set; } = null!;
    public DbSet<Worker> Workers { get; set; } = null!;
    public DbSet<CleaningRecord> CleaningRecords { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Hostel configuration
        modelBuilder.Entity<Hostel>(entity =>
        {
            entity.HasKey(e => e.HostelId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasMany(e => e.Users).WithOne(u => u.Hostel).HasForeignKey(u => u.HostelId);
            entity.HasMany(e => e.Rooms).WithOne(r => r.Hostel).HasForeignKey(r => r.HostelId);
            entity.HasMany(e => e.Students).WithOne(s => s.Hostel).HasForeignKey(s => s.HostelId);
            entity.HasMany(e => e.Complaints).WithOne(c => c.Hostel).HasForeignKey(c => c.HostelId);
            entity.HasMany(e => e.Fees).WithOne(f => f.Hostel).HasForeignKey(f => f.HostelId);
            entity.HasMany(e => e.Workers).WithOne(w => w.Hostel).HasForeignKey(w => w.HostelId);
        });
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Student).WithOne(s => s.User).HasForeignKey<Student>(s => s.UserId);
            entity.HasOne(e => e.Worker).WithOne(w => w.User).HasForeignKey<Worker>(w => w.UserId);
        });
        
        // Room configuration
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId);
            entity.Property(e => e.RoomNumber).IsRequired().HasMaxLength(50);
            entity.HasMany(e => e.Students).WithOne(s => s.Room).HasForeignKey(s => s.RoomId);
            entity.HasMany(e => e.CleaningRecords).WithOne(c => c.Room).HasForeignKey(c => c.RoomId);
        });
        
        // Student configuration
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId);
            entity.Property(e => e.RollNumber).HasMaxLength(50);
            entity.HasMany(e => e.Complaints).WithOne(c => c.Student).HasForeignKey(c => c.StudentId);
            entity.HasMany(e => e.Fees).WithOne(f => f.Student).HasForeignKey(f => f.StudentId);
        });
        
        // Complaint configuration
        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(e => e.ComplaintId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Resolution).HasMaxLength(500);
        });
        
        // Fee configuration
        modelBuilder.Entity<Fee>(entity =>
        {
            entity.HasKey(e => e.FeeId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
        });
        
        // Worker configuration
        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.WorkerId);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
            entity.HasMany(e => e.CleaningRecords).WithOne(c => c.Worker).HasForeignKey(c => c.WorkerId);
        });
        
        // CleaningRecord configuration
        modelBuilder.Entity<CleaningRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.HasIndex(e => new { e.RoomId, e.Date });
        });
    }
}
