# 🏨 Smart Hostel Management System - Backend Setup Documentation

## 📋 Overview

This document provides a comprehensive overview of the backend infrastructure for the Smart Hostel Management System built with ASP.NET Core 10.0, Entity Framework Core, and SQL Server.

---

## 🗂️ Project Structure

```
newhms/
├── Models/
│   ├── Entities/                    # Core domain entities
│   │   ├── ApplicationUser.cs       # IdentityUser extended
│   │   ├── Hostel.cs              # Multi-tenant root entity
│   │   ├── Student.cs             # Student entity
│   │   ├── Room.cs                # Room entity
│   │   ├── Worker.cs              # Worker/Staff entity
│   │   ├── Complaint.cs           # Complaint tracking
│   │   ├── Fee.cs                 # Fee management
│   │   └── CleaningRecord.cs      # Cleaning records
│   ├── DTOs/                       # Data Transfer Objects (for future use)
│   └── ErrorViewModel.cs           # Error handling model
│
├── Data/
│   ├── ApplicationDbContext.cs     # EF Core DbContext with Identity
│   └── DatabaseSeeder.cs           # Database initialization & seeding
│
├── Services/
│   ├── Interfaces/
│   │   └── ICacheService.cs        # Distributed cache interface
│   └── Implementations/
│       └── CacheService.cs         # Redis cache implementation
│
├── Middleware/
│   └── ExceptionMiddleware.cs      # Global exception handling
│
├── Controllers/                    # API Controllers (minimal)
│   └── HomeController.cs
│
├── Properties/
│   └── launchSettings.json         # Launch configuration
│
├── Program.cs                      # Application startup configuration
├── appsettings.json               # Configuration
├── appsettings.Development.json   # Development configuration
└── new hms.csproj                 # Project file
```

---

## 🧠 Database Models & Relationships

### 1. **Hostel** (Root Entity - Multi-Tenant)
```csharp
- HostelId (PK)
- Name, Location, Description
- Capacity
- IsDeleted (Soft Delete)
- CreatedAt, UpdatedAt

Relationships:
- 1:N → Users
- 1:N → Rooms
- 1:N → Students
- 1:N → Workers
- 1:N → Complaints
- 1:N → Fees
```

### 2. **ApplicationUser** (Identity Extended)
```csharp
- Id (PK) - int
- UserName, Email, FullName
- HostelId (FK) - Multi-tenant linking
- IsDeleted (Soft Delete)
- CreatedAt, UpdatedAt

Relationships:
- N:1 → Hostel
- 1:0 or 1 → Student
- 1:0 or 1 → Worker
- Has Roles (Admin, Student, Worker)
```

### 3. **Student**
```csharp
- StudentId (PK)
- UserId (FK) - 1:1 with ApplicationUser
- HostelId (FK) - Multi-tenant
- RoomId (FK) - nullable (not assigned initially)
- RollNumber (unique per hostel)
- AdmissionDate
- IsDeleted (Soft Delete)

Relationships:
- 1:1 → ApplicationUser
- N:1 → Room
- N:1 → Hostel
- 1:N → Complaints
- 1:N → Fees
```

### 4. **Room**
```csharp
- RoomId (PK)
- HostelId (FK) - Multi-tenant
- RoomNumber (unique per hostel)
- Capacity, CurrentOccupancy
- IsDeleted (Soft Delete)

Relationships:
- N:1 → Hostel
- 1:N → Students
- 1:N → CleaningRecords
```

### 5. **Worker**
```csharp
- WorkerId (PK)
- UserId (FK) - 1:1 with ApplicationUser
- HostelId (FK) - Multi-tenant
- Department (Cleaning, Maintenance, etc.)
- JoinDate
- IsDeleted (Soft Delete)

Relationships:
- 1:1 → ApplicationUser
- N:1 → Hostel
- 1:N → CleaningRecords
```

### 6. **Complaint**
```csharp
- ComplaintId (PK)
- StudentId (FK)
- HostelId (FK) - Multi-tenant
- Title, Description
- Status (Pending, In Progress, Resolved, Closed)
- Resolution, ReportedDate, ResolvedDate
- IsDeleted (Soft Delete)

Relationships:
- N:1 → Student
- N:1 → Hostel
```

### 7. **Fee**
```csharp
- FeeId (PK)
- StudentId (FK)
- HostelId (FK) - Multi-tenant
- Amount (decimal with 2 places)
- Status (Pending, Paid, Overdue)
- DueDate, PaidDate
- TransactionId
- IsDeleted (Soft Delete)

Relationships:
- N:1 → Student
- N:1 → Hostel
```

### 8. **CleaningRecord**
```csharp
- RecordId (PK)
- RoomId (FK)
- WorkerId (FK) - nullable
- Date
- Status (Pending, Cleaned, Not Needed)
- Remarks, CleanedAt
- IsDeleted (Soft Delete)

Relationships:
- N:1 → Room
- N:0 or 1 → Worker
```

---

## 🔐 Authentication & Authorization

### Identity Setup
- **Framework**: ASP.NET Core Identity with Entity Framework
- **User Type**: `ApplicationUser : IdentityUser<int>`
- **Key ID Type**: `int` (not string) for better performance

### Roles Implemented
```
1. Admin           - Full system access, user management
2. Student         - Can view complaints, fees, room allocation
3. Worker          - Can update cleaning records, view assigned rooms
```

### JWT Configuration
- **Token Type**: Bearer JWT
- **Secret Key**: Configured in `appsettings.json`
- **Issuer**: SmartHostelAPI
- **Audience**: SmartHostelClient
- **Default Expiration**: 24 hours

### Role-Based Authorization
```csharp
// Example usage in controllers (future):
[Authorize(Roles = "Admin")]
[Authorize(Roles = "Admin,Worker")]
[Authorize(Roles = "Student")]
```

---

## 🧱 Multi-Tenant Support

### Implementation Strategy
- **Tenant Root**: `Hostel` entity serves as the tenant root
- **Tenant Key**: `HostelId` present on all major entities
- **Isolation Level**: Data is filtered by `HostelId` at query time

### Multi-Tenant Features
1. **Global Query Filters**
   - Automatically filters entities by `HostelId`
   - Soft delete filters implemented via `IsDeleted` flag

2. **Unique Constraints**
   - `(HostelId, Email)` unique on Users
   - `(HostelId, RoomNumber)` unique on Rooms
   - `(HostelId, RollNumber)` unique on Students

3. **Cascade Behavior**
   - Hostel deletion cascades to related entities
   - Student-Room relationship: SetNull (flexible room assignment)
   - Worker-CleaningRecord: SetNull (records retained if worker deleted)

### Usage Example
```csharp
// All queries automatically filtered by HostelId
var students = await context.Students
    .Where(s => s.HostelId == hostelId)
    .ToListAsync();
```

---

## 🌱 Database Seeding

### Auto-Seeding on Startup
Database seeding runs automatically in `Program.cs`:
```csharp
// Applied during application startup
context.Database.Migrate();
await DatabaseSeeder.SeedDatabaseAsync(context, userManager, roleManager, logger);
```

### Seeded Data

#### 1. Roles
- Admin, Student, Worker

#### 2. Hostels (3 sample)
- Boys Hostel A (100 capacity)
- Girls Hostel B (80 capacity)
- Senior Hostel C (50 capacity)

#### 3. Default Admin User
- Email: `admin@hms.com`
- Password: `Admin@123`
- Role: Admin

#### 4. Worker Users (3)
- worker1@hms.com, worker2@hms.com, worker3@hms.com
- Password: `Worker@123`
- Assigned to different hostels

#### 5. Student Users (4)
- student1@hms.com, student2@hms.com, student3@hms.com, student4@hms.com
- Password: `Student@123`
- Automatically assigned to rooms

#### 6. Rooms
- 30 total rooms (10 per hostel)
- Varying capacities (2-4 students per room)

#### 7. Cleaning Records
- One record per room for today
- Statuses: Pending/Cleaned (alternating)

#### 8. Complaints
- 2 sample complaints from students

#### 9. Fees
- Monthly hostel fee of ₹5000 per student
- Status: Pending

---

## ⚙️ Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartHostelDB;Trusted_Connection=true;TrustServerCertificate=true;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-change-this-in-production-must-be-at-least-32-characters",
    "Issuer": "SmartHostelAPI",
    "Audience": "SmartHostelClient",
    "ExpiresInHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "SmartHostelManagementSystem": "Debug"
    }
  }
}
```

### Dependency Injection Setup
```csharp
// DbContext
services.AddDbContext<ApplicationDbContext>();

// Identity
services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Authentication
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });

// Authorization
services.AddAuthorization();

// Caching
services.AddStackExchangeRedisCache(options => { ... });

// Services
services.AddScoped<ICacheService, CacheService>();
```

---

## 🚀 Redis Caching Setup

### Configuration
- **Default TTL**: 60 minutes
- **Connection**: Configurable via `appsettings.json`
- **Default Connection**: `localhost:6379`

### ICacheService Interface
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task ClearAsync();
}
```

### Usage Example
```csharp
// Get from cache
var user = await cacheService.GetAsync<ApplicationUser>("user:123");

// Set in cache
await cacheService.SetAsync("user:123", user, TimeSpan.FromHours(1));

// Remove from cache
await cacheService.RemoveAsync("user:123");
```

### Key Naming Conventions (Recommended)
```
user:{userId}
hostel:{hostelId}
room:{roomId}
student:{studentId}
```

---

## 🧱 Middleware & Core Setup

### 1. Exception Middleware
**Purpose**: Global exception handling for consistent error responses

**Features**:
- Catches all unhandled exceptions
- Logs exceptions with ILogger
- Returns standardized error response

**Exception Mapping**:
- `ArgumentNullException` → 400 Bad Request
- `ArgumentException` → 400 Bad Request
- `UnauthorizedAccessException` → 401 Unauthorized
- `KeyNotFoundException` → 404 Not Found
- **Default** → 500 Internal Server Error

**Response Format**:
```json
{
  "success": false,
  "message": "An unexpected error occurred",
  "errors": ["error details"]
}
```

### 2. CORS Configuration
**Policy**: AllowAll
- Allows any origin
- Allows any HTTP method
- Allows any header

**Note**: Configure restrictively for production!

### 3. HTTP Pipeline Order (Program.cs)
```
1. Exception Middleware     - Top priority for error handling
2. HTTPS Redirection        - Force secure connections
3. CORS                     - Handle cross-origin requests
4. Authentication           - Verify user identity
5. Authorization            - Check permissions
6. Controllers              - Route to endpoints
```

### 4. Logging Configuration
- **Console Logger**: Logs to console
- **Debug Logger**: Logs to debug output
- **Log Level**:
  - Default: Information
  - Microsoft.AspNetCore: Warning
  - SmartHostelManagementSystem: Debug

---

## 🔧 Project Configuration Details

### Database
- **Provider**: SQL Server
- **Connection String**: LocalDB (development)
- **Automatic Migrations**: Applied on startup
- **Seeding**: Automatic on first run

### Identity Configuration
- **Primary Key Type**: `int`
- **Token Providers**: Default providers enabled
- **Password Requirements**: Default ASP.NET Core requirements

### Swagger/OpenAPI
- **Endpoint**: `/swagger`
- **Default Route**: `/` (redirects to Swagger UI)
- **Documentation**: Auto-generated from controllers

---

## 📦 NuGet Dependencies

### Core Framework
- `Microsoft.AspNetCore.OpenApi`
- `Swashbuckle.AspNetCore`

### Database & ORM
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`

### Authentication & Security
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `System.IdentityModel.Tokens.Jwt`

### Caching
- `StackExchange.Redis`

### Logging
- `Serilog.AspNetCore`

### Utilities
- `FluentValidation.AspNetCore`
- `Newtonsoft.Json`
- `AutoMapper.Extensions.Microsoft.DependencyInjection`

---

## 🎯 Key Features Implemented

### ✅ Models & Database
- [x] 8 core entities with proper relationships
- [x] Foreign key constraints
- [x] Soft delete flag (`IsDeleted`) on all major entities
- [x] Global query filters for soft delete
- [x] Unique constraints for data integrity
- [x] Timestamps (`CreatedAt`, `UpdatedAt`)

### ✅ Authentication & Authorization
- [x] ASP.NET Core Identity setup
- [x] JWT bearer token authentication
- [x] 3 roles: Admin, Student, Worker
- [x] Role-based authorization ready
- [x] Email-based user identification

### ✅ Multi-Tenant Support
- [x] `HostelId` linking on all entities
- [x] Data isolation at query level
- [x] Unique constraints per tenant
- [x] Cascade deletion handling

### ✅ Database Seeding
- [x] Automatic seeding on application startup
- [x] Default admin user
- [x] Sample roles, hostels, users
- [x] Sample rooms, workers, students
- [x] Sample complaints, fees, cleaning records
- [x] Proper logging of seeding operations

### ✅ Project Configuration
- [x] DbContext setup with Identity
- [x] Connection string configuration
- [x] Dependency injection setup
- [x] JWT configuration
- [x] CORS configuration

### ✅ Redis Caching
- [x] Distributed cache configuration
- [x] `ICacheService` interface
- [x] `CacheService` implementation
- [x] Configurable TTL
- [x] Exception handling in cache operations

### ✅ Middleware & Core Setup
- [x] Global exception middleware
- [x] Logging setup (Console, Debug)
- [x] CORS setup
- [x] Session ready (not enabled by default)
- [x] Swagger/OpenAPI documentation

---

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- SQL Server (or LocalDB)
- Redis (for caching, optional for dev)

### Setup Steps

1. **Update Connection String** (if needed)
   ```json
   // appsettings.json
   "DefaultConnection": "your-connection-string"
   ```

2. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```
   Or it runs automatically on first startup.

3. **Build the Project**
   ```bash
   dotnet build
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the Application**
   - API Documentation: `https://localhost:5001`
   - Health Check: `GET /`

### Default Credentials

| Role   | Email                | Password     |
|--------|----------------------|--------------|
| Admin  | admin@hms.com       | Admin@123    |
| Worker | worker1@hms.com     | Worker@123   |
| Student| student1@hms.com    | Student@123  |

---

## 📝 Best Practices Implemented

1. **Dependency Injection**: All services registered in IoC container
2. **Soft Deletes**: Non-destructive data deletion with `IsDeleted` flag
3. **Multi-Tenancy**: Data isolation per hostel with query filters
4. **Exception Handling**: Global middleware for consistent error handling
5. **Logging**: Structured logging with ILogger
6. **Security**: JWT authentication with role-based authorization
7. **Timestamps**: Audit trails with `CreatedAt` and `UpdatedAt`
8. **Caching**: Redis integration for performance optimization
9. **Code Organization**: Clean separation of concerns (Models, Data, Services)
10. **Documentation**: XML comments on all public members

---

## 🔜 Next Steps

### Ready for Implementation
1. **Business Logic Layer**: Create service classes for business operations
2. **API Endpoints**: Implement REST controllers for CRUD operations
3. **DTOs**: Create request/response DTOs for API contracts
4. **Validation**: Add FluentValidation rules
5. **Unit Tests**: Create test projects with xUnit/NUnit
6. **Integration Tests**: Test API endpoints and database operations

### Performance Optimization
1. Implement caching strategies
2. Add database indexing for common queries
3. Implement pagination for list operations
4. Add query optimization with includes

### Security Enhancements
1. Implement rate limiting
2. Add API key authentication for services
3. Implement audit logging
4. Add data encryption for sensitive fields

---

## 📚 Resources & References

- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [JWT Authentication](https://tools.ietf.org/html/rfc7519)
- [Redis Caching](https://redis.io/)
- [CORS Configuration](https://learn.microsoft.com/en-us/aspnet/core/security/cors)

---

## 📞 Support & Questions

For questions or issues related to the backend setup, refer to the inline code comments and documentation strings in each component.

**Last Updated**: April 2026
**Version**: 1.0
**Framework**: ASP.NET Core 10.0
**Database**: SQL Server (EF Core Code First)
