# 🏫 Smart Hostel Management System - ASP.NET Core Web API

A comprehensive, production-ready hostel management system built with ASP.NET Core 10.0, featuring multi-hostel support, cleaning management module, Redis caching, and JWT authentication.

## 🎯 Project Overview

This system manages multiple hostels on a single platform with support for:
- **Multi-Hostel Architecture** - Isolated data per hostel using HostelId (Multi-tenant design)
- **Role-Based Access Control** - Admin, Student, Worker roles with JWT authentication
- **Cleaning Management** - Daily cleaning tracking with worker assignment
- **Fee Management** - Student fee tracking and payment receipts
- **Complaint System** - Student complaint management with status tracking
- **Redis Caching** - Optimized performance with distributed caching
- **Database Seeding** - Automatic data initialization on startup

## 🛠 Tech Stack

- **Framework**: ASP.NET Core 10.0
- **Language**: C# 12
- **Database**: SQL Server (LocalDB for development)
- **Authentication**: JWT Bearer Tokens
- **Caching**: Redis
- **API Documentation**: Swagger/OpenAPI
- **ORM**: Entity Framework Core 10.0

## 📦 Project Structure

```
hms/
├── Controllers/              # API endpoints
│   ├── AuthController.cs
│   ├── HostelController.cs
│   ├── RoomController.cs
│   ├── StudentController.cs
│   ├── ComplaintController.cs
│   ├── FeeController.cs
│   ├── CleaningController.cs
│   └── DashboardController.cs
│
├── Services/
│   ├── Interfaces/          # Service contracts
│   │   ├── IAuthService.cs
│   │   ├── IHostelService.cs
│   │   ├── IRoomService.cs
│   │   ├── IStudentService.cs
│   │   ├── IComplaintService.cs
│   │   ├── IFeeService.cs
│   │   ├── ICleaningService.cs
│   │   ├── IDashboardService.cs
│   │   └── ICacheService.cs
│   │
│   └── Implementations/      # Service implementations with business logic
│       ├── AuthService.cs
│       ├── HostelService.cs
│       ├── RoomService.cs
│       ├── StudentService.cs
│       ├── ComplaintService.cs
│       ├── FeeService.cs
│       ├── CleaningService.cs
│       ├── DashboardService.cs
│       └── CacheService.cs
│
├── Models/
│   ├── Entities/           # Database entities
│   │   ├── Hostel.cs
│   │   ├── User.cs
│   │   ├── Room.cs
│   │   ├── Student.cs
│   │   ├── Complaint.cs
│   │   ├── Fee.cs
│   │   ├── Worker.cs
│   │   └── CleaningRecord.cs
│   │
│   └── DTOs/              # Data Transfer Objects
│       ├── AuthDto.cs
│       ├── HostelDto.cs
│       ├── RoomDto.cs
│       ├── StudentDto.cs
│       ├── ComplaintDto.cs
│       ├── FeeDto.cs
│       ├── CleaningDto.cs
│       └── CommonDto.cs
│
├── Data/                  # Database context & seeding
│   ├── AppDbContext.cs
│   └── DatabaseSeeder.cs
│
├── Middleware/
│   └── ExceptionMiddleware.cs
│
├── Program.cs            # Application configuration
├── appsettings.json      # Configuration settings
├── appsettings.Development.json
└── hms.csproj           # Project file with dependencies
```

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- SQL Server (LocalDB) or SQL Server Express
- Redis server (for caching)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone/Open the project**
   ```bash
   cd hms
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update Database Connection**
   Edit `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartHostelDB;Trusted_Connection=true;TrustServerCertificate=true;"
   }
   ```

4. **Ensure Redis is running**
   ```bash
   redis-server
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

The API will start at `https://localhost:5001`

## 📚 API Documentation

### Base URL
```
https://localhost:5001/api/v1
```

### Authentication
All endpoints (except login/register) require JWT Bearer token in the Authorization header:
```
Authorization: Bearer <your-token-here>
```

### API Endpoints

#### 🔐 Authentication (`/auth`)
- `POST /auth/register` - Register new user
- `POST /auth/login` - Login and get JWT token
- `GET /auth/hello` - Test endpoint

#### 🏢 Hostel Management (`/hostel`)
- `POST /hostel` - Create hostel (Admin only)
- `GET /hostel` - Get all hostels (Admin only)
- `GET /hostel/{id}` - Get hostel details
- `GET /hostel/{id}/stats` - Get hostel statistics
- `PUT /hostel/{id}` - Update hostel (Admin only)
- `DELETE /hostel/{id}` - Delete hostel (Admin only)

#### 🛏️ Room Management (`/room`)
- `POST /room` - Create room (Admin only)
- `GET /room` - Get all rooms
- `GET /room/{id}` - Get room details
- `GET /room/available/list` - Get available rooms
- `PUT /room/{id}` - Update room (Admin only)
- `DELETE /room/{id}` - Delete room (Admin only)

#### 👨‍🎓 Student Management (`/student`)
- `POST /student` - Create student (Admin only)
- `GET /student` - Get all students (Admin only)
- `GET /student/{id}` - Get student details
- `POST /student/{id}/allocate-room` - Allocate room to student (Admin only)
- `GET /student/room/{roomId}` - Get students in room
- `DELETE /student/{id}` - Remove student (Admin only)

#### 📝 Complaint Management (`/complaint`)
- `POST /complaint` - Create complaint (Student only)
- `GET /complaint` - Get all complaints (Admin only)
- `GET /complaint/{id}` - Get complaint details
- `GET /complaint/status/{status}` - Get complaints by status (Admin only)
- `PUT /complaint/{id}` - Update complaint (Admin only)
- `DELETE /complaint/{id}` - Delete complaint (Admin only)

#### 💰 Fee Management (`/fee`)
- `POST /fee` - Create fee (Admin only)
- `GET /fee` - Get all fees (Admin only)
- `GET /fee/{id}` - Get fee details
- `GET /fee/student/{studentId}` - Get student fees
- `POST /fee/{id}/pay` - Mark fee as paid
- `GET /fee/{id}/receipt` - Get fee receipt

#### 🧹 Cleaning Management (`/cleaning`)
- `POST /cleaning/mark` - Mark room as cleaned (Worker only)
- `GET /cleaning/today` - Get today's cleaning report
- `GET /cleaning/pending` - Get pending cleaning rooms
- `GET /cleaning/history/room/{roomId}` - Get room cleaning history
- `GET /cleaning/date-range` - Get cleaning records by date range (Admin only)

#### 📊 Dashboard (`/dashboard`)
- `GET /dashboard/admin` - Get admin dashboard (Admin only)
- `GET /dashboard/hostel/stats` - Get hostel quick statistics

## 🔑 Sample API Responses

### Login Response
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "userId": 1,
      "name": "Admin User",
      "email": "admin@hms.com",
      "role": "Admin",
      "hostelId": 1
    }
  }
}
```

### Get Hostel Stats Response
```json
{
  "success": true,
  "message": "Hostel statistics retrieved successfully",
  "data": {
    "hostelId": 1,
    "hostelName": "Boys Hostel A",
    "totalRooms": 10,
    "occupiedRooms": 8,
    "totalStudents": 20,
    "totalComplaints": 3,
    "pendingComplaints": 1,
    "totalFeesPending": 50000.00
  }
}
```

### Dashboard Response
```json
{
  "success": true,
  "message": "Dashboard retrieved successfully",
  "data": {
    "totalHostels": 3,
    "totalStudents": 120,
    "totalRooms": 50,
    "occupiedRooms": 45,
    "totalComplaints": 15,
    "pendingComplaints": 5,
    "totalFeesPending": 500000.00,
    "totalFeesCollected": 2500000.00,
    "pendingCleaningRooms": 8,
    "cleanedRoomsToday": 42,
    "cleaningPercentageToday": 84.0,
    "hostelStats": [...]
  }
}
```

## 🌱 Database Seeding

The application automatically seeds the database on startup with:
- **3 Sample Hostels** with different capacities
- **1 Admin User** (email: admin@hms.com, password: Admin@123)
- **3 Workers** assigned to cleaning duties
- **4 Sample Students** with room allocations
- **10 Rooms per Hostel** with varying capacities
- **Sample Complaints & Fees**
- **Daily Cleaning Records**

### Test Credentials
```
Admin:
  Email: admin@hms.com
  Password: Admin@123

Student:
  Email: student1@hms.com
  Password: Student@123

Worker:
  Email: worker1@hms.com
  Password: Worker@123
```

## 🔐 Authentication & Authorization

### JWT Token Claims
```csharp
- UserId: User's unique identifier
- Email: User's email address
- Role: User's role (Admin, Student, Worker)
- HostelId: Assigned hostel identifier
```

### Role-Based Access Control
```csharp
[Authorize(Roles = "Admin")]        // Admin only
[Authorize(Roles = "Student")]      // Student only
[Authorize(Roles = "Worker")]       // Worker only
[Authorize(Roles = "Admin,Student")] // Admin or Student
```

## 💾 Redis Caching Implementation

The system caches frequently accessed data:

```csharp
// Cache Keys Pattern
- hostel_{hostelId}              // 1 hour TTL
- hostel_{hostelId}_stats        // 5 minutes TTL
- room_{roomId}                  // 1 hour TTL
- room_available_{hostelId}      // 30 minutes TTL
- student_{studentId}            // 1 hour TTL
- complaint_{complaintId}        // 1 hour TTL
- fee_{feeId}                    // 1 hour TTL
- cleaning_today_{hostelId}      // 30 minutes TTL
- dashboard_admin_{hostelId}     // 5 minutes TTL
```

## 🗄️ Database Schema

### Entities with Relationships
```
Hostel (1) ─→ (Many) User
         ├─→ (Many) Room
         ├─→ (Many) Student
         ├─→ (Many) Complaint
         ├─→ (Many) Fee
         └─→ (Many) Worker

User (1) ─→ (0-1) Student
     ├─→ (0-1) Worker
     └─→ (Many) Hostel

Room (1) ─→ (Many) Student
     └─→ (Many) CleaningRecord

Student (1) ─→ (Many) Complaint
        └─→ (Many) Fee

Worker (1) ─→ (Many) CleaningRecord

CleaningRecord (Many) ─→ (1) Room
               └─→ (1) Worker
```

### Soft Delete
All major entities include `IsDeleted` flag for soft deletion (logical delete).

## 🧠 Key Features Implemented

✅ **Multi-Hostel Support** - Filter all queries by HostelId  
✅ **JWT Authentication** - Secure token-based access  
✅ **Role-Based Authorization** - Three role system (Admin, Student, Worker)  
✅ **API Versioning** - `/api/v1/` routing  
✅ **Redis Caching** - Distributed cache for performance  
✅ **Database Seeding** - Automatic data initialization  
✅ **Exception Handling** - Global exception middleware  
✅ **Logging** - ILogger integration throughout  
✅ **Model Validation** - Data annotations validation  
✅ **Soft Delete** - Logical deletion support  
✅ **Cleaning Management** - Daily cleaning tracking  
✅ **Room Capacity Validation** - Prevent over-allocation  
✅ **Fee Management** - Payment tracking with receipts  
✅ **Complaint System** - Status tracking and resolution  
✅ **Dashboard Statistics** - Real-time analytics  
✅ **CORS** - Cross-origin resource sharing enabled  

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=SmartHostelDB;..."
  },
  "Jwt": {
    "SecretKey": "your-secret-key-change-in-production",
    "Issuer": "SmartHostelManagementSystem",
    "Audience": "SmartHostelAPI",
    "ExpiresInHours": "24"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

## 📊 Database Migrations

### Create Migration
```bash
dotnet ef migrations add InitialCreate
```

### Update Database
```bash
dotnet ef database update
```

## 🧪 Testing the API

### Using Swagger UI
1. Navigate to `https://localhost:5001`
2. Click "Authorize" button
3. Enter Bearer token from login response
4. Test endpoints interactively

### Using curl
```bash
# Login
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@hms.com","password":"Admin@123"}'

# Get Hostels
curl -X GET https://localhost:5001/api/v1/hostel \
  -H "Authorization: Bearer <token>"
```

## 🚨 Common Issues & Troubleshooting

### Issue: Redis Connection Failed
**Solution**: Ensure Redis is running
```bash
redis-server  # Start Redis
```

### Issue: Database Connection Failed
**Solution**: Update connection string in appsettings.json and ensure SQL Server is running

### Issue: Migration Errors
**Solution**: Clear migrations and start fresh
```bash
dotnet ef database drop --force
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 📝 Notes

- **Security**: Change JWT SecretKey before production deployment
- **Passwords**: All test passwords are hashed using SHA256
- **Emails**: Unique email constraint prevents duplicate registrations
- **Cache Invalidation**: Cache automatically invalidated on data changes
- **Soft Delete**: All deletions are soft (logical), data not actually removed

## 🎓 Learning Points

This project demonstrates:
- Clean architecture with Service layer
- Dependency injection and IoC
- JWT authentication and authorization
- Redis caching with TTL
- Entity relationships in EF Core
- Global exception handling
- API versioning
- CORS configuration
- Database seeding strategies
- Role-based access control
- Multi-tenant application design

## 📄 License

This project is for educational purposes.

---

**Created**: April 2026  
**Framework**: ASP.NET Core 10.0  
**Status**: Production-Ready ✅
