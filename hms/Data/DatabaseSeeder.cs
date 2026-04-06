using Microsoft.EntityFrameworkCore;
using SmartHostelManagementSystem.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace SmartHostelManagementSystem.Data;

/// <summary>
/// Service for seeding initial data into the database
/// </summary>
public class DatabaseSeeder
{
    private static readonly string DefaultAdminPassword = "Admin@123";
    private static readonly string DefaultWorkerPassword = "Worker@123";
    private static readonly string DefaultStudentPassword = "Student@123";
    
    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    public static async Task SeedDatabaseAsync(AppDbContext context)
    {
        try
        {
            // Apply any pending migrations
            await context.Database.MigrateAsync();
            
            // Skip if data already exists
            if (context.Hostels.Any())
            {
                return;
            }
            
            // Seed Hostels
            var hostels = SeedHostels(context);
            await context.SaveChangesAsync();
            
            // Seed Users (Admin, Workers, Students)
            var users = SeedUsers(context, hostels);
            await context.SaveChangesAsync();
            
            // Seed Rooms
            var rooms = SeedRooms(context, hostels);
            await context.SaveChangesAsync();
            
            // Seed Workers
            SeedWorkers(context, users);
            await context.SaveChangesAsync();
            
            // Seed Students
            SeedStudents(context, users, rooms);
            await context.SaveChangesAsync();
            
            // Seed Cleaning Records
            SeedCleaningRecords(context, users, rooms);
            await context.SaveChangesAsync();
            
            // Seed Complaints
            SeedComplaints(context);
            await context.SaveChangesAsync();
            
            // Seed Fees
            SeedFees(context);
            await context.SaveChangesAsync();
            
            Console.WriteLine("✅ Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error seeding database: {ex.Message}");
            throw;
        }
    }
    
    private static List<Hostel> SeedHostels(AppDbContext context)
    {
        var hostels = new List<Hostel>
        {
            new Hostel
            {
                Name = "Boys Hostel A",
                Location = "Block A, Campus",
                Description = "Main hostel for boys",
                Capacity = 200,
                CreatedAt = DateTime.UtcNow
            },
            new Hostel
            {
                Name = "Girls Hostel B",
                Location = "Block B, Campus",
                Description = "Main hostel for girls",
                Capacity = 180,
                CreatedAt = DateTime.UtcNow
            },
            new Hostel
            {
                Name = "Senior Hostel C",
                Location = "Block C, Campus",
                Description = "Hostel for senior students",
                Capacity = 150,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        context.Hostels.AddRange(hostels);
        return hostels;
    }
    
    private static List<User> SeedUsers(AppDbContext context, List<Hostel> hostels)
    {
        var users = new List<User>
        {
            // Admin user
            new User
            {
                Name = "Admin User",
                Email = "admin@hms.com",
                PhoneNumber = "9876543210",
                PasswordHash = HashPassword(DefaultAdminPassword),
                Role = "Admin",
                HostelId = hostels[0].HostelId,
                CreatedAt = DateTime.UtcNow
            },
            
            // Workers
            new User
            {
                Name = "Rajesh Kumar",
                Email = "worker1@hms.com",
                PhoneNumber = "8765432109",
                PasswordHash = HashPassword(DefaultWorkerPassword),
                Role = "Worker",
                HostelId = hostels[0].HostelId,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Name = "Priya Sharma",
                Email = "worker2@hms.com",
                PhoneNumber = "7654321098",
                PasswordHash = HashPassword(DefaultWorkerPassword),
                Role = "Worker",
                HostelId = hostels[0].HostelId,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Name = "Suresh Patel",
                Email = "worker3@hms.com",
                PhoneNumber = "6543210987",
                PasswordHash = HashPassword(DefaultWorkerPassword),
                Role = "Worker",
                HostelId = hostels[1].HostelId,
                CreatedAt = DateTime.UtcNow
            },
            
            // Students
            new User
            {
                Name = "Arjun Singh",
                Email = "student1@hms.com",
                PhoneNumber = "9123456789",
                PasswordHash = HashPassword(DefaultStudentPassword),
                Role = "Student",
                HostelId = hostels[0].HostelId,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Name = "Neha Verma",
                Email = "student2@hms.com",
                PhoneNumber = "9234567891",
                PasswordHash = HashPassword(DefaultStudentPassword),
                Role = "Student",
                HostelId = hostels[0].HostelId,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Name = "Vikram Gupta",
                Email = "student3@hms.com",
                PhoneNumber = "9345678912",
                PasswordHash = HashPassword(DefaultStudentPassword),
                Role = "Student",
                HostelId = hostels[1].HostelId,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Name = "Anjali Desai",
                Email = "student4@hms.com",
                PhoneNumber = "9456789123",
                PasswordHash = HashPassword(DefaultStudentPassword),
                Role = "Student",
                HostelId = hostels[1].HostelId,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        context.Users.AddRange(users);
        return users;
    }
    
    private static List<Room> SeedRooms(AppDbContext context, List<Hostel> hostels)
    {
        var rooms = new List<Room>();
        
        // Add rooms for each hostel
        for (int h = 0; h < hostels.Count; h++)
        {
            for (int i = 1; i <= 10; i++)
            {
                rooms.Add(new Room
                {
                    RoomNumber = $"{h + 1}{i:D2}",
                    Capacity = i % 2 == 0 ? 4 : 2,
                    CurrentOccupancy = 0,
                    HostelId = hostels[h].HostelId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        
        context.Rooms.AddRange(rooms);
        return rooms;
    }
    
    private static void SeedWorkers(AppDbContext context, List<User> users)
    {
        var workers = new List<Worker>
        {
            new Worker
            {
                UserId = users[1].UserId, // Rajesh Kumar
                HostelId = users[1].HostelId,
                Department = "Cleaning",
                JoinDate = DateTime.UtcNow.AddMonths(-6),
                CreatedAt = DateTime.UtcNow
            },
            new Worker
            {
                UserId = users[2].UserId, // Priya Sharma
                HostelId = users[2].HostelId,
                Department = "Cleaning",
                JoinDate = DateTime.UtcNow.AddMonths(-4),
                CreatedAt = DateTime.UtcNow
            },
            new Worker
            {
                UserId = users[3].UserId, // Suresh Patel
                HostelId = users[3].HostelId,
                Department = "Cleaning",
                JoinDate = DateTime.UtcNow.AddMonths(-3),
                CreatedAt = DateTime.UtcNow
            }
        };
        
        context.Workers.AddRange(workers);
    }
    
    private static void SeedStudents(AppDbContext context, List<User> users, List<Room> rooms)
    {
        var students = new List<Student>
        {
            new Student
            {
                UserId = users[4].UserId, // Arjun Singh
                RoomId = rooms[0].RoomId,
                HostelId = users[4].HostelId,
                RollNumber = "ENG001",
                AdmissionDate = DateTime.UtcNow.AddMonths(-12),
                CreatedAt = DateTime.UtcNow
            },
            new Student
            {
                UserId = users[5].UserId, // Neha Verma
                RoomId = rooms[1].RoomId,
                HostelId = users[5].HostelId,
                RollNumber = "ENG002",
                AdmissionDate = DateTime.UtcNow.AddMonths(-10),
                CreatedAt = DateTime.UtcNow
            },
            new Student
            {
                UserId = users[6].UserId, // Vikram Gupta
                RoomId = rooms[10].RoomId,
                HostelId = users[6].HostelId,
                RollNumber = "ENG003",
                AdmissionDate = DateTime.UtcNow.AddMonths(-8),
                CreatedAt = DateTime.UtcNow
            },
            new Student
            {
                UserId = users[7].UserId, // Anjali Desai
                RoomId = rooms[11].RoomId,
                HostelId = users[7].HostelId,
                RollNumber = "ENG004",
                AdmissionDate = DateTime.UtcNow.AddMonths(-6),
                CreatedAt = DateTime.UtcNow
            }
        };
        
        context.Students.AddRange(students);
        
        // Update room occupancy
        foreach (var student in students)
        {
            if (student.RoomId.HasValue)
            {
                var room = rooms.FirstOrDefault(r => r.RoomId == student.RoomId.Value);
                if (room != null)
                {
                    room.CurrentOccupancy++;
                }
            }
        }
    }
    
    private static void SeedCleaningRecords(AppDbContext context, List<User> users, List<Room> rooms)
    {
        var today = DateTime.UtcNow.Date;
        var cleaningRecords = new List<CleaningRecord>();
        
        // Create cleaning records for today for each room
        for (int i = 0; i < Math.Min(5, rooms.Count); i++)
        {
            cleaningRecords.Add(new CleaningRecord
            {
                RoomId = rooms[i].RoomId,
                WorkerId = context.Workers.First().WorkerId,
                Date = today,
                Status = i % 2 == 0 ? "Cleaned" : "Pending",
                Remarks = i % 2 == 0 ? "Room cleaned and sanitized" : null,
                CleanedAt = i % 2 == 0 ? today.AddHours(10) : null,
                CreatedAt = today
            });
        }
        
        context.CleaningRecords.AddRange(cleaningRecords);
    }
    
    private static void SeedComplaints(AppDbContext context)
    {
        var students = context.Students.Take(2).ToList();
        if (!students.Any()) return;
        
        var complaints = new List<Complaint>
        {
            new Complaint
            {
                StudentId = students[0].StudentId,
                HostelId = students[0].HostelId,
                Title = "Water leakage in room",
                Description = "There is a water leakage from the ceiling in the room. Please repair it as soon as possible.",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Complaint
            {
                StudentId = students[1].StudentId,
                HostelId = students[1].HostelId,
                Title = "Maintenance issue",
                Description = "The ceiling fan in the room is not working. Please send someone to fix it.",
                Status = "In Progress",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };
        
        context.Complaints.AddRange(complaints);
    }
    
    private static void SeedFees(AppDbContext context)
    {
        var students = context.Students.ToList();
        if (!students.Any()) return;
        
        var fees = new List<Fee>();
        var today = DateTime.UtcNow;
        
        foreach (var student in students)
        {
            fees.Add(new Fee
            {
                StudentId = student.StudentId,
                HostelId = student.HostelId,
                Amount = 5000,
                Status = "Pending",
                DueDate = today.AddMonths(1),
                CreatedAt = today
            });
        }
        
        context.Fees.AddRange(fees);
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
}
