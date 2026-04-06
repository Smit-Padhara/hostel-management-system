# 🏗️ System Architecture & Design

## Overall Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                         │
│ (Web App / Mobile App / Third-party Services)                │
└────────────────────────┬────────────────────────────────────┘
                         │
                   HTTP/HTTPS
                         │
┌─────────────────────────┴────────────────────────────────────┐
│                    API GATEWAY LAYER                          │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  • CORS Configuration                               │   │
│  │  • Rate Limiting (AspNetCoreRateLimit)              │   │
│  │  • Global Exception Handling                        │   │
│  │  • Request/Response Logging                         │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────┬────────────────────────────────────┘
                         │
┌─────────────────────────┴────────────────────────────────────┐
│                  AUTHENTICATION LAYER                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  JWT Token Generation & Validation                  │   │
│  │  Claims: UserId, Email, Role, HostelId             │   │
│  │  Token Expiration: 24 hours                         │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────┬────────────────────────────────────┘
                         │
┌─────────────────────────┴────────────────────────────────────┐
│                   CONTROLLER LAYER                            │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ AuthController  │ HostelController  │ RoomController  │ │
│  │ StudentController │ ComplaintController │ FeeController│ │
│  │ CleaningController │ DashboardController              │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────┬────────────────────────────────────┘
                         │
┌─────────────────────────┴────────────────────────────────────┐
│                  SERVICE LAYER (Business Logic)              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  ├─ IAuthService → AuthService                       │ │
│  │  ├─ IHostelService → HostelService                   │ │
│  │  ├─ IRoomService → RoomService                       │ │
│  │  ├─ IStudentService → StudentService                 │ │
│  │  ├─ IComplaintService → ComplaintService             │ │
│  │  ├─ IFeeService → FeeService                         │ │
│  │  ├─ ICleaningService → CleaningService               │ │
│  │  ├─ IDashboardService → DashboardService             │ │
│  │  └─ ICacheService → CacheService                     │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────┬──────────────────────────────┬────────────────┘
              │                              │
        ┌─────▼──────┐             ┌────────▼───────┐
        │  DATABASE  │             │      REDIS     │
        │   LAYER    │             │    CACHE       │
        └────────────┘             └────────────────┘
```

## 📊 Multi-Tenant Architecture

```
┌───────────────────────────────────────────────────────────┐
│                    SINGLE DEPLOYMENT                       │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  Hostel 1 (HostelId: 1)                             │ │
│  │  ├─ Data isolated via HostelId filter              │ │
│  │  ├─ Rooms: 101-110                                 │ │
│  │  ├─ Students: 20                                   │ │
│  │  └─ Workers: 2                                     │ │
│  └──────────────────────────────────────────────────────┘ │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  Hostel 2 (HostelId: 2)                             │ │
│  │  ├─ Data isolated via HostelId filter              │ │
│  │  ├─ Rooms: 201-210                                 │ │
│  │  ├─ Students: 18                                   │ │
│  │  └─ Workers: 2                                     │ │
│  └──────────────────────────────────────────────────────┘ │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  Hostel 3 (HostelId: 3)                             │ │
│  │  ├─ Data isolated via HostelId filter              │ │
│  │  ├─ Rooms: 301-310                                 │ │
│  │  ├─ Students: 22                                   │ │
│  │  └─ Workers: 2                                     │ │
│  └──────────────────────────────────────────────────────┘ │
│                                                             │
└───────────────────────────────────────────────────────────┘
```

## 🔐 Security Layers

```
1. AUTHENTICATION
   ├─ JWT Token Validation
   ├─ Token Expiration Check
   └─ Secret Key Verification

2. AUTHORIZATION
   ├─ Role-Based Access Control (RBAC)
   │  ├─ [Authorize(Roles = "Admin")]
   │  ├─ [Authorize(Roles = "Student")]
   │  └─ [Authorize(Roles = "Worker")]
   └─ Multi-tenant isolation via HostelId

3. DATA VALIDATION
   ├─ Model-level validation (DataAnnotations)
   ├─ Business logic validation
   └─ Soft delete (IsDeleted flag)

4. ERROR HANDLING
   ├─ Global Exception Middleware
   ├─ Standardized Error Responses
   └─ Request Logging
```

## 💾 Caching Strategy

```
CACHE HIERARCHY
├─ L1: In-Memory Cache (Optional)
└─ L2: Redis Distributed Cache
    ├─ Hostel Data (1 hour TTL)
    ├─ Room Data (1 hour TTL)
    ├─ Student Data (1 hour TTL)
    ├─ Cleaning Records (30 min TTL)
    ├─ Dashboard Stats (5 min TTL)
    └─ Available Rooms (30 min TTL)

INVALIDATION STRATEGY
├─ Manual: On data modification
├─ TTL: Automatic expiration
└─ Pattern: Prefix-based cleanup
```

## 🗄️ Database Design

### Entity Relationships

```
Hostel (One to Many)
├─ → User (HostelId FK)
├─ → Room (HostelId FK)
├─ → Student (HostelId FK)
├─ → Complaint (HostelId FK)
├─ → Fee (HostelId FK)
└─ → Worker (HostelId FK)

User (One to One)
├─ → Student (UserId FK)
└─ → Worker (UserId FK)

Room (One to Many)
├─ → Student (RoomId FK)
└─ → CleaningRecord (RoomId FK)

Student (One to Many)
├─ → Complaint (StudentId FK)
└─ → Fee (StudentId FK)

Worker (One to Many)
└─ → CleaningRecord (WorkerId FK)

CleaningRecord (Many to One)
├─ → Room (RoomId FK)
└─ → Worker (WorkerId FK)
```

### Indexing Strategy

```
PRIMARY KEYS
├─ HostelId
├─ UserId
├─ RoomId
├─ StudentId
├─ ComplaintId
├─ FeeId
├─ WorkerId
└─ RecordId

FOREIGN KEYS (Automatically indexed)
├─ User.HostelId
├─ Room.HostelId
├─ Student.HostelId
├─ Complaint.HostelId
├─ Fee.HostelId
├─ Worker.HostelId
├─ CleaningRecord.RoomId
└─ CleaningRecord.WorkerId

CUSTOM INDEXES
├─ User (Email) - Unique Index
├─ User (HostelId, Role)
├─ CleaningRecord (RoomId, Date)
└─ Complaint (StudentId, Status)
```

## 📈 Performance Optimization

### Database Optimization
```
1. Query Optimization
   ├─ Eager Loading (.Include())
   ├─ Projection (Select())
   └─ Pagination (Skip/Take)

2. Index Strategy
   ├─ Foreign Key Indexes
   ├─ Search Column Indexes
   └─ Composite Indexes for common queries

3. Connection Pooling
   ├─ Default: 100 connections
   └─ Configurable in connection string
```

### Caching Strategy
```
1. What to Cache
   ├─ Hostel data (frequently accessed, changes rarely)
   ├─ Room availability (high traffic)
   ├─ Dashboard statistics (expensive calculation)
   └─ Cleaning reports (requested multiple times)

2. Cache Duration
   ├─ Short-lived: 5-30 minutes (frequently changing data)
   ├─ Medium-lived: 1 hour (stable data)
   └─ Long-lived: 24 hours (static data)

3. Cache Invalidation
   ├─ Event-driven: Invalidate on create/update
   ├─ Time-based: TTL expiration
   └─ Manual: Admin clear cache
```

## 🔄 Data Flow Example: Student Allocation

```
1. CLIENT SENDS REQUEST
   POST /api/v1/student/1/allocate-room
   {
     "roomId": 3
   }

2. CONTROLLER RECEIVES
   StudentController.AllocateRoomAsync()
   ├─ Extracts HostelId from JWT claims
   └─ Passes to service layer

3. SERVICE LAYER PROCESSES
   StudentService.AllocateRoomAsync()
   ├─ Validates student exists in hostel
   ├─ Validates room exists and has capacity
   ├─ Updates student.RoomId
   ├─ Increments room.CurrentOccupancy
   ├─ Saves to database
   └─ Invalidates cache

4. DATABASE UPDATES
   UPDATE Students SET RoomId = 3, UpdatedAt = NOW()
   UPDATE Rooms SET CurrentOccupancy = 2, UpdatedAt = NOW()

5. CACHE INVALIDATION
   Remove cache keys:
   ├─ student_1
   ├─ student_all_1
   └─ room_available_1

6. RESPONSE SENT TO CLIENT
   {
     "success": true,
     "message": "Room allocated successfully",
     "data": { ... }
   }
```

## 🚨 Error Handling Flow

```
1. ERROR OCCURS
   └─ Exception thrown in service/controller

2. GLOBAL MIDDLEWARE CATCHES
   └─ ExceptionMiddleware

3. MIDDLEWARE PROCESSES
   ├─ Logs error details
   ├─ Maps exception type to HTTP status
   └─ Creates standardized response

4. RESPONSE SENT
   {
     "success": false,
     "message": "User-friendly message",
     "errors": ["Technical error details"]
   }
```

## 📋 Request/Response Cycle

```
REQUEST:
┌─ Headers
│  ├─ Authorization: Bearer <JWT>
│  └─ Content-Type: application/json
├─ Body (if POST/PUT)
│  └─ JSON payload
└─ Query Parameters

VALIDATION:
├─ JWT validation
├─ Role authorization
├─ Model validation
└─ Business logic validation

PROCESSING:
├─ Controller validation
├─ Service layer execution
├─ Database operation
└─ Cache update

RESPONSE:
{
  "success": true/false,
  "message": "Message",
  "data": {...},
  "errors": [...]
}
```

## 🎯 Design Patterns Used

### 1. Service Layer Pattern
```csharp
Controller → Service (Interface) → Implementation
```

### 2. Repository Pattern (Implicit)
```csharp
Services use DbContext directly for data access
```

### 3. Dependency Injection
```csharp
builder.Services.AddScoped<IService, Implementation>();
```

### 4. Factory Pattern
```csharp
Services create DTOs from entities
```

### 5. Strategy Pattern
```csharp
ICacheService with Redis implementation
```

### 6. Decorator Pattern
```csharp
Services wrap business logic with logging
```

## 🔧 Configuration Management

```
appsettings.json
├─ ConnectionStrings
│  └─ DefaultConnection
├─ Jwt
│  ├─ SecretKey
│  ├─ Issuer
│  ├─ Audience
│  └─ ExpiresInHours
├─ Redis
│  └─ ConnectionString
└─ RateLimit
   ├─ EnableEndpointRateLimiting
   ├─ HttpStatusCode
   └─ RateLimitPolicies

appsettings.Development.json
└─ Environment-specific overrides
```

## 📊 API Versioning

```
Current Version: v1.0
URL Pattern: /api/v1/<resource>

Future Versions:
├─ /api/v2/<resource> (Breaking changes)
└─ /api/v3/<resource> (Major updates)
```

## 🧪 Testing Pyramid

```
        ▲
       ╱ ╲
      ╱   ╲     Integration Tests
     ╱─────╲    (API Endpoints)
    ╱       ╲
   ╱─────────╲   Unit Tests
  ╱           ╲  (Services)
 ╱─────────────╲
╱_______________╲ E2E Tests
```

---

**Architecture Version**: 1.0  
**Last Updated**: April 2026  
**Status**: Production Ready ✅
