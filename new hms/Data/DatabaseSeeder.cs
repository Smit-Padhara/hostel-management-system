using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace SmartHostelManagementSystem.Data;

/// <summary>
/// Database seeding service - initializes data on application startup
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedDatabaseAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        ILogger<ApplicationDbContext> logger)
    {
        try
        {
            logger.LogInformation("Starting database seeding...");
            
            // ============= SEED ROLES =============
            await SeedRolesAsync(roleManager, logger);
            
            // ============= SEED HOSTELS =============
            await SeedHostelsAsync(context, logger);
            
            // ============= SEED USERS AND ROLES =============
            await SeedUsersAsync(context, userManager, logger);
            
            // ============= SEED ROOMS =============
            await SeedRoomsAsync(context, logger);
            
            // ============= SEED WORKERS =============
            await SeedWorkersAsync(context, logger);
            
            // ============= SEED STUDENTS =============
            await SeedStudentsAsync(context, logger);
            
            // ============= SEED CLEANING RECORDS =============
            await SeedCleaningRecordsAsync(context, logger);
            
            // ============= SEED COMPLAINTS =============
            await SeedComplaintsAsync(context, logger);
            
            // ============= SEED FEES =============
            await SeedFeesAsync(context, logger);
            
            logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during database seeding");
            throw;
        }
    }
    
    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole<int>> roleManager,
        ILogger<ApplicationDbContext> logger)
    {
        var roles = new[] { "Admin", "Student", "Worker" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                logger.LogInformation("Role '{Role}' created", role);
            }
        }
    }
    
    private static async Task SeedHostelsAsync(
        ApplicationDbContext context,
        ILogger<ApplicationDbContext> logger)
    {
        if (context.Hostels.Any())
            return;
        
        var hostels = new[]
        {
            new Hostel { Name = "Boys Hostel A", Location = "Building A, Floor 1", Capacity = 100, Description = "Premium boys hostel" },
            new Hostel { Name = "Girls Hostel B", Location = "Building B, Floor 1", Capacity = 80, Description = "Girls hostel with modern amenities" },
            new Hostel { Name = "Senior Hostel C", Location = "Building C, Floor 2", Capacity = 50, Description = "Senior hostel for final year students" }
        };
        
        context.Hostels.AddRange(hostels);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} hostels", hostels.Length);
    }
    
    private static async Task SeedUsersAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ApplicationDbContext> logger)
    {
        if (context.Users.Any())
            return;
        
        var hostels = await context.Hostels.ToListAsync();
        if (!hostels.Any())
            return;
        
        // Admin User
        var adminUser = new ApplicationUser
        {
            UserName = "admin@hms.com",
            Email = "admin@hms.com",
            FullName = "System Admin",
            HostelId = hostels[0].HostelId,
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
        logger.LogInformation("Admin user created");
        
        // Worker Users
        var workerUsers = new[]
        {
            new ApplicationUser { UserName = "worker1@hms.com", Email = "worker1@hms.com", FullName = "John Worker", HostelId = hostels[0].HostelId, EmailConfirmed = true },
            new ApplicationUser { UserName = "worker2@hms.com", Email = "worker2@hms.com", FullName = "Mike Worker", HostelId = hostels[1].HostelId, EmailConfirmed = true },
            new ApplicationUser { UserName = "worker3@hms.com", Email = "worker3@hms.com", FullName = "Sarah Worker", HostelId = hostels[2].HostelId, EmailConfirmed = true }
        };
        
        foreach (var worker in workerUsers)
        {
            await userManager.CreateAsync(worker, "Worker@123");
            await userManager.AddToRoleAsync(worker, "Worker");
        }
        logger.LogInformation("Worker users created");
        
        // Student Users
        var studentUsers = new[]
        {
            new ApplicationUser { UserName = "student1@hms.com", Email = "student1@hms.com", FullName = "Alice Student", HostelId = hostels[0].HostelId, EmailConfirmed = true },
            new ApplicationUser { UserName = "student2@hms.com", Email = "student2@hms.com", FullName = "Bob Student", HostelId = hostels[0].HostelId, EmailConfirmed = true },
            new ApplicationUser { UserName = "student3@hms.com", Email = "student3@hms.com", FullName = "Carol Student", HostelId = hostels[1].HostelId, EmailConfirmed = true },
            new ApplicationUser { UserName = "student4@hms.com", Email = "student4@hms.com", FullName = "David Student", HostelId = hostels[2].HostelId, EmailConfirmed = true }
        };
        
        foreach (var student in studentUsers)
        {
            await userManager.CreateAsync(student, "Student@123");
            await userManager.AddToRoleAsync(student, "Student");
        }
        logger.LogInformation("Student users created");
        
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedRoomsAsync(
        ApplicationDbContext context,
        ILogger<ApplicationDbContext> logger)
    {
        if (context.Rooms.Any())
            return;
        
        var hostels = await context.Hostels.ToListAsync();
        var rooms = new List<Room>();
        
        // Create 10 rooms per hostel
        foreach (var hostel in hostels)
        {
            for (int i = 1; i <= 10; i++)
            {
                rooms.Add(new Room
                {
                    HostelId = hostel.HostelId,
                    RoomNumber = $"{hostel.HostelId}{i:D2}",
                    Capacity = i % 2 == 0 ? 4 : 2,
                    CurrentOccupancy = 0
                });
            }
        }
        
        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} rooms", rooms.Count);
    }
    
    private static async Task SeedWorkersAsync(
        ApplicationDbContext context,
        ILogger<ApplicationDbContext> logger)
    {
        if (context.Workers.Any())
            return;
        
        var workerUsers = await context.Users.Where(u => u.UserName!.StartsWith("worker")).ToListAsync();
        var hostels = await context.Hostels.ToListAsync();
        
        var workers = workerUsers.Select((user, index) => new Worker
        {
            UserId = user.Id,
            HostelId = hostels[index % hostels.Count].HostelId,
            Department = "Cleaning",
            JoinDate = DateTime.UtcNow.AddMonths(-index)
        }).ToList();
        
        context.Workers.AddRange(workers);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} workers", workers.Count);
    }
    
    private static async Task SeedStudentsAsync(
        ApplicationDbContext context,
        ILogger<ApplicationDbContext> logger)
    {
        if (context.Students.Any())
            return;
        
        var studentUsers = await context.Users.Where(u => u.UserName!.StartsWith("student")).ToListAsync();
        var rooms = await context.Rooms.ToListAsync();
        
        var students = new List<Student>();
        
        foreach (var (user, index) in studentUsers.Select((u, i) => (u, i)))
        {
            var room = rooms.FirstOrDefault(r => r.HostelId == user.HostelId && r.CurrentOccupancy < r.Capacity);
            
            var student = new Student
            {
                UserId = user.Id,
                HostelId = user.HostelId,
                RoomId = room?.RoomId,
                RollNumber = $"ROLL{2024001 + index}",
                AdmissionDate = DateTime.UtcNow.AddMonths(-index)
            };
            
            students.Add(student);
            
            // Update room occupancy
            if (room != null)
            {
                room.CurrentOccupancy++;
            }
        }
        
        context.Students.AddRange(students);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} students", students.Count);
    }
    
    private static async Task SeedCleaningRecordsAsync(
        ApplicationDbContext context,
        ILogger<ApplicationDbContext> logger)
    {
        if (context.CleaningRecords.Any(c => c.Date.Date == DateTime.UtcNow.Date))
            return;
        
        var rooms = await context.Rooms.ToListAsync();
        var workers = await context.Workers.ToListAsync();
        var today = DateTime.UtcNow.Date;
        
        var records = new List<CleaningRecord>();
        
        foreach (var (room, index) in rooms.Select((r, i) => (r, i)))
        {
            var worker = workers.FirstOrDefault(w => w.HostelId == room.HostelId);
            
            records.Add(new CleaningRecord
            {
                RoomId = room.RoomId,
                WorkerId = worker?.WorkerId,
                Date = today,
                Status = index % 3 == 0 ? "Cleaned" : "Pending",
                CleanedAt = index % 3 == 0 ? DateTime.UtcNow : null
            });
        }
        
        context.CleaningRecords.AddRange(records);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} cleaning records for today", records.Count);
    }
    
    private static async Task SeedComplaintsAsync(
        ApplicationDbContext context,
        ILogger<ApplicationDbContext> logger)
    {
        if (context.Complaints.Any())
            return;
        
        var students = await context.Students.ToListAsync();
        
        var complaints = new[]
        {
            new Complaint { StudentId = students[0].StudentId, HostelId = students[0].HostelId, Title = "Water Leakage", Description = "Water leaking from ceiling", Status = "Pending" },
            new Complaint { StudentId = students[1].StudentId, HostelId = students[1].HostelId, Title = "Electricity Issue", Description = "Power cut in room", Status = "In Progress" }
        };
        
        context.Complaints.AddRange(complaints);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded sample complaints");
    }
    
    private static async Task SeedFeesAsync(
        ApplicationDbContext context,
        ILogger<ApplicationDbContext> logger)
    {
        if (context.Fees.Any())
            return;
        
        var students = await context.Students.ToListAsync();
        
        var fees = students.Select(s => new Fee
        {
            StudentId = s.StudentId,
            HostelId = s.HostelId,
            Amount = 5000.00m,
            Status = "Pending",
            DueDate = DateTime.UtcNow.AddMonths(1)
        }).ToList();
        
        context.Fees.AddRange(fees);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded fees for all students");
    }
}
