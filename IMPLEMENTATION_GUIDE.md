# Smart Hostel Management System - Business Logic & Controllers

## 🎯 Project Overview

This document describes the complete implementation of the Smart Hostel Management System's business logic layer with 7 production-ready services and 6 MVC controllers following best practices.

---

## 📁 Project Structure

```
newhms/
├── DTOs/
│   ├── Requests/              # Request models for API input
│   │   ├── CreateHostelRequest.cs
│   │   ├── CreateRoomRequest.cs
│   │   ├── AssignStudentRequest.cs
│   │   ├── RaiseComplaintRequest.cs
│   │   ├── UpdateComplaintStatusRequest.cs
│   │   ├── RecordFeeRequest.cs
│   │   └── UpdateCleaningStatusRequest.cs
│   └── Responses/             # Response DTOs for API output
│       ├── HostelDto.cs
│       ├── RoomDto.cs
│       ├── StudentDto.cs
│       ├── ComplaintDto.cs
│       ├── FeeDto.cs
│       ├── CleaningRecordDto.cs
│       └── DashboardSummaryDto.cs
├── Services/
│   ├── Interfaces/            # Service contracts
│   │   ├── IHostelService.cs
│   │   ├── IRoomService.cs
│   │   ├── IStudentService.cs
│   │   ├── IComplaintService.cs
│   │   ├── IFeeService.cs
│   │   ├── ICleaningService.cs
│   │   └── IDashboardService.cs
│   └── Implementations/       # Service implementations
│       ├── HostelService.cs
│       ├── RoomService.cs
│       ├── StudentService.cs
│       ├── ComplaintService.cs
│       ├── FeeService.cs
│       ├── CleaningService.cs
│       └── DashboardService.cs
├── Controllers/               # MVC Controllers
│   ├── HostelController.cs
│   ├── RoomController.cs
│   ├── StudentController.cs
│   ├── ComplaintController.cs
│   ├── CleaningController.cs
│   ├── FeeController.cs
│   └── DashboardController.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # Dependency Injection setup
```

---

## 🔧 Setup & Installation

### 1. **Register Services in Program.cs**

Add the following code to your `Program.cs`:

```csharp
// After existing configurations
builder.Services.AddApplicationServices();

// If using AutoMapper
builder.Services.AddApplicationAutoMapper();
```

### 2. **Complete Program.cs Example**

```csharp
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Extensions;
using SmartHostelManagementSystem.Services.Interfaces;
using SmartHostelManagementSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add Application Services
builder.Services.AddApplicationServices();

// Add Cache Service (existing)
builder.Services.AddScoped<ICacheService, CacheService>();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 📚 Service Layer Overview

### 1. **IHostelService** - Hostel Management
Creates, manages, and retrieves hostel information.

**Key Features:**
- Create hostels with validation
- Get hostel details with statistics
- Update hostel information
- Soft delete hostels
- Redis caching for performance

**Methods:**
```csharp
Task<HostelDto> CreateHostelAsync(CreateHostelRequest request)
Task<HostelDto?> GetHostelByIdAsync(int hostelId)
Task<IEnumerable<HostelDto>> GetAllHostelsAsync()
Task<HostelDto?> GetHostelWithStatsAsync(int hostelId)
Task<HostelDto?> UpdateHostelAsync(int hostelId, CreateHostelRequest request)
Task<bool> DeleteHostelAsync(int hostelId)
```

---

### 2. **IRoomService** - Room Management with Capacity Control ⭐
Manages rooms with comprehensive capacity checking and over-allocation prevention.

**Key Business Rules:**
- ✅ **Prevent over-allocation** - Rooms cannot exceed capacity
- ✅ **Capacity validation** - Check available seats before assignment
- ✅ **Room status tracking** - Full/Empty/Partially occupied
- ✅ **Occupancy management** - Increment/Decrement on student assignment

**Methods:**
```csharp
Task<RoomDto> CreateRoomAsync(CreateRoomRequest request)
Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(int hostelId)  // Only rooms with capacity
Task<int> CheckRoomCapacityAsync(int roomId)                    // Available seats
Task<bool> IsRoomFullAsync(int roomId)                          // Business Rule check
Task<int> IncrementOccupancyAsync(int roomId)                   // Validate before increment
Task<int> DecrementOccupancyAsync(int roomId)
```

---

### 3. **IStudentService** - Student Management with Assignment ⭐
Handles student assignment with multi-level validation and capacity checks.

**Key Business Rules:**
- ✅ **Capacity validation** - Cannot assign to full room
- ✅ **Hostel filtering** - Student must belong to same hostel as room
- ✅ **Assignment prevention** - Cannot reassign without unassigning first
- ✅ **Over-allocation prevention** - Critical validation

**Methods:**
```csharp
Task<StudentDto?> AssignStudentToRoomAsync(AssignStudentRequest request)  // Complex validation
Task<IEnumerable<StudentDto>> GetUnassignedStudentsAsync(int hostelId)
Task<bool> UnassignStudentAsync(int studentId)
Task<bool> IsStudentAssignedAsync(int studentId)
Task<int> GetStudentCountAsync(int hostelId)
```

---

### 4. **IComplaintService** - Complaint Management
Manages student complaints with status tracking and resolution timestamps.

**Key Business Rules:**
- ✅ **Status tracking** - Pending → In Progress → Resolved → Closed
- ✅ **Resolution tracking** - Timestamp recorded when resolved
- ✅ **Hostel filtering** - Sort by hostel for multi-tenancy

**Methods:**
```csharp
Task<ComplaintDto> RaiseComplaintAsync(RaiseComplaintRequest request)
Task<IEnumerable<ComplaintDto>> GetComplaintsByStatusAsync(int hostelId, string status)
Task<ComplaintDto?> UpdateComplaintStatusAsync(int complaintId, UpdateComplaintStatusRequest request)
Task<int> GetPendingComplaintsCountAsync(int hostelId)
```

---

### 5. **IFeeService** - Fee & Payment Management
Tracks fees, payments, and generates receipts with collection metrics.

**Key Business Rules:**
- ✅ **Payment status** - Pending → Partial → Paid
- ✅ **Overdue tracking** - Identifies overdue payments
- ✅ **Receipt generation** - Unique receipt numbers
- ✅ **Collection metrics** - Total collected, pending, overdue

**Methods:**
```csharp
Task<FeeDto> RecordFeeAsync(RecordFeeRequest request)
Task<IEnumerable<FeeDto>> GetPendingFeesAsync(int hostelId)
Task<IEnumerable<FeeDto>> GetOverdueFeesAsync(int hostelId)
Task<(FeeDto feeDto, string receiptNumber)> RecordPaymentAsync(int feeId, decimal amount, DateTime date)
Task<Dictionary<string, object>> GenerateReceiptAsync(int feeId, decimal amount, DateTime date)
Task<decimal> GetTotalPendingFeesAsync(int hostelId)
```

---

### 6. **ICleaningService** - Cleaning Management (CORE MODULE) ⭐⭐⭐
Comprehensive cleaning task management with daily tracking and reporting.

**Core Requirements Met:**
- ✅ Mark room as Cleaned / Pending
- ✅ Get today's cleaning report
- ✅ Get pending rooms
- ✅ Cleaning history
- ✅ Identify uncleaned rooms
- ✅ Daily status tracking

**Methods:**
```csharp
Task<CleaningRecordDto> CreateCleaningRecordAsync(int roomId, DateTime date)
Task<IEnumerable<CleaningRecordDto>> GetTodaysCleaningTasksAsync(int hostelId)    // CORE
Task<IEnumerable<CleaningRecordDto>> GetPendingCleaningRecordsAsync(int hostelId) // CORE
Task<int> GetPendingCleaningCountAsync(int hostelId)                             // CORE
Task<CleaningRecordDto?> MarkRoomAsCleanedAsync(int roomId, int? workerId, string? remarks)
Task<IEnumerable<CleaningRecordDto>> GetCleaningHistoryAsync(int roomId, int days) // CORE
Task<IEnumerable<CleaningRecordDto>> GetCleaningReportAsync(int hostelId, DateTime date) // CORE
Task<IEnumerable<CleaningRecordDto>> GetUncleanlRoomsAsync(int hostelId, int daysThreshold) // CORE
```

**Example Usage:**
```csharp
// Get today's tasks
var tasks = await cleaningService.GetTodaysCleaningTasksAsync(hostelId);

// Mark completed
await cleaningService.MarkRoomAsCleanedAsync(roomId, workerId, "Room cleaned thoroughly");

// Get pending rooms
var pending = await cleaningService.GetPendingCleaningRecordsAsync(hostelId);

// Get history for a room
var history = await cleaningService.GetCleaningHistoryAsync(roomId, days: 30);
```

---

### 7. **IDashboardService** - Analytics & Reporting
Aggregates data from all services with Redis caching for performance.

**Dashboard Metrics:**
- Total/occupied/available rooms
- Room occupancy percentage
- Student count and distribution
- Complaint statistics and resolution rates
- Cleaning status and pending rooms
- Fee collection rates and overdue amounts
- Critical alerts

**Methods:**
```csharp
Task<DashboardSummaryDto> GetDashboardSummaryAsync(int hostelId)         // All metrics
Task<IEnumerable<ComplaintDto>> GetRecentComplaintsAsync(int hostelId)
Task<Dictionary<string, object>> GetCriticalAlertsAsync(int hostelId)
Task<Dictionary<string, object>> GetUtilizationMetricsAsync(int hostelId)
Task<Dictionary<string, object>> GetComplaintMetricsAsync(int hostelId)
Task<Dictionary<string, object>> GetFeeMetricsAsync(int hostelId)
Task CacheDashboardDataAsync(int hostelId)
Task InvalidateDashboardCacheAsync(int hostelId)
```

---

## 🎮 API Controllers

### **1. HostelController**
Base URL: `api/hostel`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/` | Create hostel |
| GET | `/` | Get all hostels |
| GET | `/{id}` | Get hostel by ID |
| GET | `/{id}/stats` | Get hostel with stats |
| PUT | `/{id}` | Update hostel |
| DELETE | `/{id}` | Delete hostel |

---

### **2. RoomController**
Base URL: `api/room`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/` | Create room |
| GET | `/{id}` | Get room by ID |
| GET | `/hostel/{hostelId}` | Get all rooms in hostel |
| GET | `/available/{hostelId}` | Get available rooms (IMPORTANT) |
| GET | `/{id}/capacity` | Check room capacity |
| GET | `/{id}/full` | Check if room is full |
| PUT | `/{id}` | Update room |
| DELETE | `/{id}` | Delete room |

---

### **3. StudentController**
Base URL: `api/student`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/{id}` | Get student by ID |
| GET | `/hostel/{hostelId}` | Get all students in hostel |
| GET | `/room/{roomId}` | Get students in room |
| GET | `/unassigned/{hostelId}` | Get unassigned students |
| POST | `/assign` | Assign student to room (WITH CAPACITY CHECK) |
| POST | `/{id}/unassign` | Unassign student |
| GET | `/count/{hostelId}` | Get student count |
| GET | `/{id}/assigned` | Check if assigned |

**Critical:** Assign endpoint validates capacity and prevents over-allocation!

---

### **4. ComplaintController**
Base URL: `api/complaint`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/` | Raise complaint |
| GET | `/{id}` | Get complaint by ID |
| GET | `/hostel/{hostelId}` | Get complaints in hostel |
| GET | `/hostel/{hostelId}/status/{status}` | Filter by status |
| GET | `/student/{studentId}` | Get student's complaints |
| PUT | `/{id}/status` | Update status |
| GET | `/pending-count/{hostelId}` | Get pending count |
| DELETE | `/{id}` | Delete complaint |

---

### **5. CleaningController** (CORE MODULE)
Base URL: `api/cleaning`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/` | Create cleaning record |
| GET | `/{id}` | Get record by ID |
| GET | `/room/{roomId}` | Get records for room |
| GET | `/today/{hostelId}` | Today's tasks (CORE) |
| GET | `/pending/{hostelId}` | Pending records (CORE) |
| GET | `/pending-count/{hostelId}` | Pending count (CORE) |
| PUT | `/{id}/status` | Update status |
| POST | `/mark-cleaned/{roomId}` | Mark as cleaned |
| GET | `/history/{roomId}` | Cleaning history (CORE) |
| GET | `/report/{hostelId}` | Cleaning report (CORE) |
| GET | `/uncleaned/{hostelId}` | Uncleaned rooms (CORE) |
| DELETE | `/{id}` | Delete record |

---

### **6. FeeController**
Base URL: `api/fee`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/` | Record fee |
| GET | `/{id}` | Get fee by ID |
| GET | `/student/{studentId}` | Get fees for student |
| GET | `/hostel/{hostelId}` | Get fees in hostel |
| GET | `/pending/{hostelId}` | Get pending fees |
| GET | `/overdue/{hostelId}` | Get overdue fees |
| POST | `/{id}/payment` | Record payment |
| GET | `/total-pending/{hostelId}` | Total pending |
| GET | `/total-collected/{hostelId}` | Total collected |
| GET | `/students-with-pending/{hostelId}` | Count with pending |
| GET | `/{id}/receipt` | Generate receipt |

---

### **7. DashboardController**
Base URL: `api/dashboard`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/{hostelId}` | Complete dashboard summary |
| GET | `/{hostelId}/recent-complaints` | Recent complaints |
| GET | `/{hostelId}/alerts` | Critical alerts |
| GET | `/{hostelId}/utilization` | Room utilization metrics |
| GET | `/{hostelId}/complaint-metrics` | Complaint statistics |
| GET | `/{hostelId}/fee-metrics` | Fee collection stats |
| POST | `/{hostelId}/cache` | Cache dashboard data |
| DELETE | `/{hostelId}/cache` | Invalidate cache |

---

## 💾 Redis Caching

All services implement strategic caching with TTL:

```csharp
// Hostel caching: 1 hour
await _cacheService.SetAsync($"hostel_{id}", hostel, TimeSpan.FromHours(1));

// Room availability: 15 minutes (frequently changes)
await _cacheService.SetAsync($"available_rooms_{hostelId}", rooms, TimeSpan.FromMinutes(15));

// Dashboard: 5 minutes (real-time data)
await _cacheService.SetAsync($"dashboard_{hostelId}", summary, TimeSpan.FromMinutes(5));

// Today's tasks: 1 hour (updates throughout day)
await _cacheService.SetAsync($"today_cleaning_{hostelId}_{date}", tasks, TimeSpan.FromHours(1));
```

---

## 📋 Business Rules Implemented

### **Room Over-Allocation Prevention** ⭐
```csharp
// ✅ Validated in RoomService.IncrementOccupancyAsync()
if (room.CurrentOccupancy >= room.Capacity)
    throw new InvalidOperationException("Room is at full capacity");

// ✅ Validated in StudentService.AssignStudentToRoomAsync()
if (room.CurrentOccupancy >= room.Capacity)
    throw new InvalidOperationException("Cannot assign to full room");
```

### **Hostel Filtering** ✅
```csharp
// All student queries filtered by HostelId
.Where(s => s.HostelId == hostelId && !s.IsDeleted)

// Prevents cross-hostel data access
```

### **Soft Delete Pattern** ✅
```csharp
entity.IsDeleted = true;
public bool IsDeleted { get; set; } = false;
```

### **Date-based Cleaning Tracking** ✅
```csharp
var today = DateTime.UtcNow.Date;
.Where(cr => cr.Date.Date == today)
```

---

## 🧪 Example API Flows

### **Student Assignment Flow (with validation)**
```
1. POST /api/student/assign
   {
     "studentId": 5,
     "roomId": 3
   }

2. Service validates:
   ✓ Student exists
   ✓ Not already assigned
   ✓ Room exists
   ✓ Same hostel
   ✓ Room has capacity ← CRITICAL

3. Returns 200 OK with updated student
```

### **Cleaning Report Flow**
```
1. GET /api/cleaning/today/1
   → Returns all cleaning tasks for hostel 1 today

2. GET /api/cleaning/pending/1
   → Returns uncompleted tasks

3. PUT /api/cleaning/5/status
   { "status": "Cleaned", "workerId": 2 }
   → Marks cleaned with timestamp

4. GET /api/cleaning/report/1?date=2024-01-15
   → Complete daily report
```

### **Fee Payment Flow (with receipt)**
```
1. POST /api/fee/5/payment?amount=500&paymentDate=2024-01-15
   ✓ Records payment
   ✓ Updates fee status
   ✓ Generates receipt
   → Returns receipt number: FEE-000005-20240115143022

2. GET /api/fee/5/receipt?amount=500&date=2024-01-15
   → Returns detailed receipt with all payment info
```

---

## 🔍 Data Validation

### **Request Validation Examples**

```csharp
[Required]
[StringLength(100, MinimumLength = 3)]
public string Name { get; set; }

[Range(1, 50)]
public int Capacity { get; set; }

[RegularExpression("^(Pending|Paid|Partial)$")]
public string PaymentStatus { get; set; }

[Phone]
public string? PhoneNumber { get; set; }
```

---

## 📊 Response Format

### **Success Response**
```json
{
  "roomId": 1,
  "hostelId": 1,
  "roomNumber": "101",
  "capacity": 4,
  "currentOccupancy": 3,
  "availableSeats": 1,
  "isFull": false,
  "isEmpty": false
}
```

### **Error Response**
```json
{
  "message": "Room 101 is at full capacity (4/4). Cannot assign more students."
}
```

### **Dashboard Response**
```json
{
  "totalStudents": 150,
  "totalRooms": 40,
  "occupiedRooms": 35,
  "availableRooms": 5,
  "fullRooms": 28,
  "pendingCleaningRooms": 5,
  "cleanedRoomsToday": 35,
  "outstandingFeesAmount": 45000,
  "occupancyPercentage": 87.5,
  "lastUpdated": "2024-01-15T14:30:00Z"
}
```

---

## 🚀 Performance Optimizations

1. **Redis Caching** - Strategic TTL-based caching
2. **Async/Await** - Non-blocking operations
3. **Parallel Queries** - Dashboard loads all metrics in parallel
4. **Index-based Lookups** - Fast hostel/room/student queries
5. **Soft Deletes** - Logical deletion with global query filters
6. **Cache Invalidation** - Smart cache busting on updates

---

## ✅ Testing Guidelines

### **Unit Tests to Write**
```csharp
[Test]
public async Task AssignStudent_WhenRoomFull_ThrowsException()
{
    // Arrange: Room with full capacity
    // Act & Assert: Should throw InvalidOperationException
}

[Test]
public async Task CreateRoom_WhenDuplicateNumber_ThrowsException()
{
    // Prevent room number duplicates in same hostel
}

[Test]
public async Task RecordFee_WhenPaymentExceedsBalance_ThrowsException()
{
    // Payment cannot exceed remaining balance
}
```

---

## 📝 Comments in Code

All services include:
- **Class-level XML documentation**
- **Method summaries** explaining parameters
- **Business rule comments** with ⭐ markers
- **Cache strategy comments**
- **Validation comments**

---

## 🔐 Security Considerations

1. **Input Validation** - All DTOs use Data Annotations
2. **Authorization** - Add [Authorize] attributes as needed
3. **Hostel Filtering** - Prevents cross-hostel data access
4. **Soft Deletes** - Never permanently delete
5. **Exception Handling** - Descriptive error messages

---

## 📚 Additional Notes

- **No Models Created** - Reuses existing entities from Data layer
- **No DbContext Creation** - Uses existing ApplicationDbContext
- **Identity Integration** - Leverages existing Identity setup
- **Redis **Ready** - All services use ICacheService
- **Multi-tenant Ready** - HostelId filtering throughout

---

## 🎯 Summary

✅ **7 Production-Ready Services**
✅ **6 Fully Implemented Controllers**
✅ **14 DTOs with Validation**
✅ **Core Cleaning Management Module**
✅ **Over-Allocation Prevention**
✅ **Redis Caching Throughout**
✅ **Comprehensive Comments**
✅ **Ready for UI Integration**
