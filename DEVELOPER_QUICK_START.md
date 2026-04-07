# 🚀 Developer Quick Start Guide

## Prerequisites

- **.NET SDK**: 10.0 or later
- **SQL Server**: LocalDB or full SQL Server instance
- **Visual Studio Code** or **Visual Studio 2022+**
- **Git**: For version control
- **Redis**: (Optional, for caching in development)

---

## Project Setup

### 1. Clone & Open Project

```bash
# Navigate to project directory
cd newhms

# Open in VS Code
code .

# Or Visual Studio
start "new hms.sln"
```

### 2. Restore Dependencies

```bash
# Restore NuGet packages
dotnet restore
```

### 3. Configure Database Connection

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartHostelDB;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-must-be-at-least-32-characters-long-for-production"
  }
}
```

### 4. Apply Migrations & Seed Database

Option A: **Using CLI** (automatic on first run)
```bash
# Just run the application - migrations apply automatically
dotnet run
```

Option B: **Manual migration**
```bash
# Apply pending migrations
dotnet ef database update

# Then run the app
dotnet run
```

### 5. Verify Application Startup

Expected output:
```
info: SmartHostelManagementSystem.Data.ApplicationDbContext[0]
      Starting database seeding...
info: SmartHostelManagementSystem.Data.ApplicationDbContext[0]
      Database seeding completed successfully!
Application started. Press Ctrl+C to exit.
```

---

## Access the Application

After startup, navigate to:

- **API Documentation**: `https://localhost:5001`
- **Health Check**: `https://localhost:5001/`

You'll see the Swagger UI with all available endpoints.

---

## Testing Default Login

### Admin Account
```
Email: admin@hms.com
Password: Admin@123
Role: Admin
```

### Sample Student Account
```
Email: student1@hms.com
Password: Student@123
Role: Student
```

### Sample Worker Account
```
Email: worker1@hms.com
Password: Worker@123
Role: Worker
```

---

## Common Development Tasks

### ✏️ Create a New Entity

1. **Create Model** in `Models/Entities/NewEntity.cs`:
```csharp
namespace SmartHostelManagementSystem.Models.Entities;

public class NewEntity
{
    public int NewEntityId { get; set; }
    public int HostelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public virtual Hostel? Hostel { get; set; }
}
```

2. **Add DbSet** to `Data/ApplicationDbContext.cs`:
```csharp
public DbSet<NewEntity> NewEntities => Set<NewEntity>();
```

3. **Configure in OnModelCreating**:
```csharp
modelBuilder.Entity<NewEntity>(entity =>
{
    entity.HasKey(e => e.NewEntityId);
    entity.HasQueryFilter(e => !e.IsDeleted);
    entity.HasOne(e => e.Hostel)
        .WithMany()
        .HasForeignKey(e => e.HostelId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

4. **Create Migration**:
```bash
dotnet ef migrations add AddNewEntity
dotnet ef database update
```

### ➕ Add a New Service

1. **Create Interface** in `Services/Interfaces/INewService.cs`:
```csharp
namespace SmartHostelManagementSystem.Services.Interfaces;

public interface INewService
{
    Task<bool> DoSomethingAsync(int id);
}
```

2. **Create Implementation** in `Services/Implementations/NewService.cs`:
```csharp
using SmartHostelManagementSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartHostelManagementSystem.Services.Implementations;

public class NewService : INewService
{
    private readonly ILogger<NewService> _logger;
    
    public NewService(ILogger<NewService> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> DoSomethingAsync(int id)
    {
        _logger.LogInformation("Processing item {Id}", id);
        // Implementation here
        return true;
    }
}
```

3. **Register in Program.cs**:
```csharp
builder.Services.AddScoped<INewService, NewService>();
```

### 🔐 Implement Authorization

```csharp
// In any controller method
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AdminOnly()
{
    return Ok("Admin access granted");
}

[Authorize(Roles = "Admin,Student")]
public async Task<IActionResult> AdminOrStudent()
{
    return Ok("Admin or Student access granted");
}
```

### 🗄️ Query Database with Multi-Tenant Support

```csharp
// Example in a service
public async Task<List<Student>> GetStudentsForHostelAsync(int hostelId)
{
    // Queries automatically filtered by IsDeleted=false
    var students = await _context.Students
        .Where(s => s.HostelId == hostelId)
        .Include(s => s.User)
        .Include(s => s.Room)
        .ToListAsync();
    
    return students;
}
```

### 💾 Using Cache Service

```csharp
// Inject ICacheService
public class MyService
{
    private readonly ICacheService _cacheService;
    
    public MyService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
    
    public async Task<User> GetUserAsync(int userId)
    {
        // Try cache first
        var cachedUser = await _cacheService.GetAsync<ApplicationUser>($"user:{userId}");
        if (cachedUser != null)
            return cachedUser;
        
        // Get from database
        var user = await _context.Users.FindAsync(userId);
        
        // Store in cache (1 hour TTL)
        if (user != null)
            await _cacheService.SetAsync($"user:{userId}", user, TimeSpan.FromHours(1));
        
        return user;
    }
}
```

---

## Debugging

### Enable Detailed Logging

Edit `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug",
      "Microsoft.EntityFrameworkCore": "Information",
      "SmartHostelManagementSystem": "Debug"
    }
  }
}
```

### View SQL Queries

```csharp
// In any service
_logger.LogInformation("SQL: {Query}", queryable.ToQueryString());
```

### Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| **Port already in use** | Change port in `launchSettings.json` |
| **Database connection failed** | Verify connection string in `appsettings.json` |
| **Migration conflicts** | Remove last migration and recreate: `dotnet ef migrations remove` |
| **Redis connection failed** | Install Redis or update connection string to disable caching |
| **JWT token invalid** | Ensure secret key is at least 32 characters |

---

## Running Tests

### Create a Test Project

```bash
# Create xUnit test project
dotnet new xunit -n SmartHostel.Tests

# Add reference to main project
cd SmartHostel.Tests
dotnet add reference ../new\ hms.csproj
```

### Sample Unit Test

```csharp
using Xunit;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostel.Tests.Services;

public class CacheServiceTests
{
    [Fact]
    public async Task SetAsync_ShouldStoreData()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        var logger = new Mock<ILogger<CacheService>>();
        var service = new CacheService(mockCache.Object, logger.Object);
        
        // Act
        await service.SetAsync("key", new { value = "test" });
        
        // Assert
        mockCache.Verify(x => x.SetStringAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### Run Tests

```bash
dotnet test
```

---

## Building for Production

### 1. Update Configuration

Create `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=SmartHostelDB;User Id=sa;Password=secure-password;",
    "Redis": "prod-redis:6379"
  },
  "Jwt": {
    "SecretKey": "your-production-secret-key-minimum-32-characters",
    "Issuer": "SmartHostelAPI",
    "Audience": "SmartHostelClient",
    "ExpiresInHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 2. Build Release

```bash
dotnet build -c Release
```

### 3. Publish

```bash
dotnet publish -c Release -o ./publish
```

### 4. Deploy

- Copy `publish` folder to production server
- Set environment variable: `ASPNETCORE_ENVIRONMENT=Production`
- Run: `dotnet SmartHostelManagementSystem.dll`

---

## Project Structure Best Practices

When adding new features, follow this pattern:

```
Feature: Student Management
├── Models/Entities/Student.cs        ✓ Already exists
├── Models/DTOs/
│   ├── CreateStudentDto.cs          → Add
│   ├── UpdateStudentDto.cs          → Add
│   └── StudentResponseDto.cs        → Add
├── Services/Interfaces/
│   └── IStudentService.cs           → Add
├── Services/Implementations/
│   └── StudentService.cs            → Add
├── Controllers/
│   └── StudentsController.cs        → Add (future)
└── Tests/
    └── StudentServiceTests.cs       → Add (future)
```

---

## Useful Commands

```bash
# Build solution
dotnet build

# Run application
dotnet run

# Run with specific environment
dotnet run --environment Production

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Revert migration
dotnet ef database update PreviousMigrationName

# Remove last migration
dotnet ef migrations remove

# View pending migrations
dotnet ef migrations list

# Scaffold database from existing DB
dotnet ef dbcontext scaffold "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer

# Run tests
dotnet test

# Create entity scaffolding
dotnet aspnet-codegenerator -p "new hms.csproj" controller -name ItemsController -m Item -dc ApplicationDbContext -outDir Controllers --useAsyncActions

# Watch for changes and rebuild
dotnet watch run
```

---

## Git Workflow

```bash
# Create feature branch
git checkout -b feature/student-management

# Make changes
# ...

# Commit changes
git add .
git commit -m "feat: Add student management service"

# Push to remote
git push origin feature/student-management

# Create pull request on GitHub
# Get code review
# Merge to main
```

---

## Performance Tips

1. **Use Include() for related data**
   ```csharp
   var student = await _context.Students
       .Include(s => s.User)
       .Include(s => s.Room)
       .FirstOrDefaultAsync(s => s.StudentId == id);
   ```

2. **Use async/await everywhere**
   ```csharp
   var result = await _service.GetDataAsync();
   ```

3. **Implement caching for read-heavy operations**
   ```csharp
   var data = await _cacheService.GetAsync<T>(key) 
       ?? await GetFromDatabaseAsync();
   ```

4. **Use projection for API responses**
   ```csharp
   var response = await _context.Students
       .Where(s => s.HostelId == hostelId)
       .Select(s => new StudentDto { Id = s.StudentId, Name = s.User.FullName })
       .ToListAsync();
   ```

5. **Add pagination for large datasets**
   ```csharp
   var page = await _context.Students
       .Skip((pageNumber - 1) * pageSize)
       .Take(pageSize)
       .ToListAsync();
   ```

---

## Security Checklist

- ✅ Change JWT secret key for production
- ✅ Use HTTPS in production
- ✅ Restrict CORS origins in production
- ✅ Implement rate limiting
- ✅ Use environment variables for secrets
- ✅ Enable SQL parameterization (EF Core does this by default)
- ✅ Validate all user inputs
- ✅ Use soft deletes for audit trail
- ✅ Implement audit logging
- ✅ Keep dependencies updated

---

## Support Resources

- **Microsoft Docs**: https://learn.microsoft.com/en-us/aspnet/core/
- **EF Core Documentation**: https://learn.microsoft.com/en-us/ef/core/
- **Stack Overflow**: Tag with `asp.net-core` or `entity-framework-core`
- **Official Forums**: https://github.com/dotnet/aspnetcore/discussions

---

## Next Steps

1. **Set up Git hooks** for pre-commit linting
2. **Create API contracts/DTOs** for each entity
3. **Implement business logic services** for each domain
4. **Build REST controllers** with proper validation
5. **Write comprehensive unit tests**
6. **Set up CI/CD pipeline** with GitHub Actions
7. **Configure Docker** for containerization
8. **Implement API versioning** for future compatibility

---

**Happy Coding! 🎉**
