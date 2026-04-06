# 📋 Smart Hostel Management System - Complete Setup Guide

## ✅ Project Status: READY TO RUN

All build errors have been resolved and the application is ready for deployment!

---

## 🔧 Recent Fixes Applied

### Swagger/Swagger-UI Version Compatibility
**Issue**: `System.TypeLoadException` - Method 'GetSwagger' not implemented in SwaggerGenerator

**Root Cause**: Swashbuckle.AspNetCore 6.5.0 has compatibility issues with .NET 10.0

**Solution**: Updated `Swashbuckle.AspNetCore` from 6.5.0 → **7.2.0**

### All Package Versions
```
✅ System.IdentityModel.Tokens.Jwt: 8.0.1
✅ Swashbuckle.AspNetCore: 7.2.0
✅ Serilog.AspNetCore: 9.0.0
✅ AspNetCoreRateLimit: 5.0.0
✅ Microsoft.OpenApi: 2.0.0
✅ AutoMapper.Extensions.Microsoft.DependencyInjection: 12.0.0
```

---

## 📦 Project Structure

```
hms/
├── Models/
│   ├── Entities/          (8 entity classes with relationships)
│   └── DTOs/              (8 DTO classes for API contracts)
├── Data/
│   ├── AppDbContext.cs    (EF Core DbContext)
│   └── DatabaseSeeder.cs  (Auto-population with sample data)
├── Services/
│   ├── Interfaces/        (9 service interfaces)
│   └── Implementations/   (9 service implementations)
├── Controllers/           (8 API controllers with auth)
├── Middleware/            (Global exception handling)
├── Program.cs             (Configuration & startup)
├── appsettings.json       (Database & JWT config)
├── appsettings.Development.json
├── hms.csproj             (NuGet packages)
└── Properties/
    └── launchSettings.json
```

---

## 🗄️ Database Entities

| Entity | Purpose | Records Seeded |
|--------|---------|-----------------|
| **Hostel** | Hostel locations | 3 |
| **User** | System users (Admin/Student/Worker) | 9 |
| **Room** | Hostel rooms | 30 |
| **Student** | Student records | 4 |
| **Worker** | Cleaning/maintenance staff | 3 |
| **Complaint** | Student complaints | 2 |
| **Fee** | Student fees | 4 |
| **CleaningRecord** | Daily cleaning tasks | 30 |

---

## 🔐 Authentication & Authorization

### JWT Configuration
- **Algorithm**: HS256 (HMAC with SHA-256)
- **Expiration**: 24 hours
- **Claims**: UserId, Email, Role, HostelId

### Role-Based Access Control
- **Admin**: Full system access, hostel management
- **Student**: View complaints, fees, room allocation
- **Worker**: Mark cleaning tasks, view assignments

### Default Test Credentials
```
Email: admin@hms.com
Password: Admin@123

Email: student@hms.com
Password: Student@123

Email: worker@hms.com
Password: Worker@123
```

---

## 🚀 Quick Start

### 1. Prerequisites Check
```powershell
# Check .NET version
dotnet --version  # Should be 10.0 or higher

# Check SQL Server LocalDB
sqllocaldb info

# Check Redis (optional)
redis-cli ping
```

### 2. Start Redis (Optional)
```powershell
redis-server
```

### 3. Run Application
```powershell
cd "c:\Users\md idrish\OneDrive\Desktop\HMS\hms"
dotnet run
```

### 4. Access Application
- **Home**: https://localhost:5001/
- **Swagger**: https://localhost:5001/swagger
- **API**: https://localhost:5001/api/v1/

---

## 📡 API Features

### Core Modules
1. **Authentication** - JWT-based user login/registration
2. **Hostel Management** - Multi-hostel support with statistics
3. **Room Management** - Room allocation and occupancy tracking
4. **Student Management** - Student lifecycle with room assignment
5. **Complaint Management** - Issue tracking and resolution
6. **Fee Management** - Payment tracking with receipts
7. **Cleaning Management** - Daily cleaning tasks and reports
8. **Dashboard** - Admin statistics and analytics

### API Response Format
All endpoints return standardized responses:
```json
{
  "success": true/false,
  "message": "Response message",
  "data": {...},
  "errors": ["error1", "error2"]
}
```

---

## 💾 Caching Strategy

### Redis Integration
- **TTL Strategy**:
  - Short-lived (5-30 min): Frequently changing data
  - Medium-lived (1 hour): Stable data
  - Long-lived (24 hours): Static data

### Cached Entities
- Hostel data (1 hour)
- Room availability (30 min)
- Student records (1 hour)
- Dashboard statistics (5 min)
- Cleaning reports (30 min)

---

## 📊 Database Relationships

```
Hostel (One)
  ├─ User (Many)
  ├─ Room (Many)
  ├─ Student (Many)
  ├─ Complaint (Many)
  ├─ Fee (Many)
  └─ Worker (Many)

User (One)
  ├─ Student (Zero/One)
  └─ Worker (Zero/One)

Room (One)
  ├─ Student (Many)
  └─ CleaningRecord (Many)

Student (One)
  ├─ Complaint (Many)
  └─ Fee (Many)

Worker (One)
  └─ CleaningRecord (Many)
```

---

## 🛡️ Security Features

- ✅ JWT Bearer token authentication
- ✅ Role-based authorization (Admin/Student/Worker)
- ✅ Multi-tenant data isolation via HostelId
- ✅ Soft delete for data integrity
- ✅ Global exception handling middleware
- ✅ CORS policy configuration
- ✅ Password hashing (SHA256)
- ✅ SQL Server integration with EF Core

---

## 📝 Documentation Files

| File | Purpose |
|------|---------|
| **README.md** | Project overview, setup, features |
| **QUICK_START.md** | 5-step quick setup guide |
| **API_TESTING_GUIDE.md** | Endpoint testing with examples |
| **ARCHITECTURE.md** | System design & diagrams |
| **BUILD_FIXES.md** | NuGet version resolution |
| **RUN_APPLICATION.md** | Detailed running instructions |

---

## 🧪 Testing the Application

### Using Swagger UI
1. Navigate to https://localhost:5001/swagger
2. Click "Authorize" button
3. Enter token from login response
4. Test endpoints directly

### Using cURL/PowerShell
```powershell
# Login
$response = Invoke-WebRequest `
  -Uri "https://localhost:5001/api/v1/auth/login" `
  -Method POST `
  -Body (@{email="admin@hms.com";password="Admin@123"} | ConvertTo-Json) `
  -ContentType "application/json" `
  -SkipCertificateCheck

# Get token
$token = ($response.Content | ConvertFrom-Json).data.token

# Use token in requests
$headers = @{ "Authorization" = "Bearer $token" }
Invoke-WebRequest `
  -Uri "https://localhost:5001/api/v1/hostel" `
  -Headers $headers `
  -SkipCertificateCheck
```

---

## 🔍 Troubleshooting

| Issue | Solution |
|-------|----------|
| SSL Certificate Error | Add `-SkipCertificateCheck` to PowerShell commands |
| Redis Connection Failed | Redis is optional; application works without it |
| Database Connection Error | Verify LocalDB is running: `sqllocaldb start mssqllocaldb` |
| Port 5001 Already in Use | Change port in `launchSettings.json` or kill process |
| Build Fails | Run `dotnet clean` then `dotnet restore` |

---

## 📚 Key Technologies

| Technology | Version | Purpose |
|-----------|---------|---------|
| ASP.NET Core | 10.0 | Web framework |
| C# | 12 | Programming language |
| Entity Framework Core | 10.0 | ORM |
| SQL Server | LocalDB | Database |
| Redis | Latest | Distributed caching |
| JWT | 8.0.1 | Authentication |
| Swagger/OpenAPI | 7.2.0 | API documentation |
| Serilog | 9.0.0 | Structured logging |

---

## ✨ Next Steps

1. ✅ **Read**: RUN_APPLICATION.md for detailed startup instructions
2. ✅ **Start**: `dotnet run` to launch the application
3. ✅ **Test**: Access https://localhost:5001/swagger for API testing
4. ✅ **Explore**: Try endpoints with test credentials
5. ✅ **Develop**: Extend with additional features as needed

---

## 📞 Support

For issues or questions:
1. Check the documentation files in the project root
2. Review the ARCHITECTURE.md for system design details
3. Consult API_TESTING_GUIDE.md for endpoint examples
4. Check launchSettings.json for configuration details

---

**Project**: Smart Hostel Management System v1.0  
**Status**: ✅ PRODUCTION READY  
**Last Updated**: April 6, 2026
