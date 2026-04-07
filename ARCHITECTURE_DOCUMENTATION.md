# 🏗️ System Architecture & Data Flow Documentation

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                     Client Application Layer                         │
│                    (Web/Mobile/Desktop Clients)                      │
└────────────────────────┬────────────────────────────────────────────┘
                         │ HTTP/HTTPS Requests
                         ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   API Gateway & Middleware Layer                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ • Exception Middleware (Global Error Handling)             │  │
│  │ • CORS Middleware (Cross-Origin Requests)                  │  │
│  │ • Authentication Middleware (JWT Validation)               │  │
│  │ • Authorization Middleware (Role-Based Access Control)     │  │
│  └──────────────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────────────┘
                         │
        ┌────────────────┼────────────────┐
        │                │                │
        ▼                ▼                ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│  Controllers    │ │   Services      │ │   Repositories │
│                 │ │   (Business     │ │   (Data Access)│
│ • Routes        │ │    Logic)       │ │                 │
│ • DTO Mapping   │ │                 │ │ • EF Core       │
│ • Validation    │ │ • ICacheService │ │ • Queries       │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                    │
         └───────────────────┼────────────────────┘
                             │
                ┌────────────┴────────────┐
                ▼                         ▼
        ┌──────────────────┐    ┌──────────────────┐
        │   EF Core DbCtx  │    │  Redis Cache     │
        │  & Migrations    │    │  Distributed     │
        └────────┬─────────┘    └────────┬─────────┘
                 │                       │
                 ▼                       ▼
        ┌──────────────────┐    ┌──────────────────┐
        │   SQL Server     │    │ Redis Server     │
        │   Database       │    │ (Cache)          │
        │                  │    │                  │
        │ • Tables         │    │ • Session Data   │
        │ • Stored Procs   │    │ • Query Cache    │
        │ • Indexes        │    │ • User Data      │
        └──────────────────┘    └──────────────────┘
```

---

## Multi-Tenant Architecture

### Tenant Isolation Strategy

```
┌──────────────────────────────────────────────────────────────┐
│                    Hostel (Tenant Root)                       │
│  • HostelId: Unique identifier                               │
│  • Name, Location, Capacity                                  │
│  • IsDeleted: Soft delete flag                               │
└──┬───────────────────────────────────┬──────────────────────┘
   │                                   │
   ├─ Users (HostelId)                │
   ├─ Rooms (HostelId)                ├─ Complete Data Isolation
   ├─ Students (HostelId)             │ Per Hostel
   ├─ Workers (HostelId)              │
   ├─ Complaints (HostelId)           │
   ├─ Fees (HostelId)                 │
   └─ CleaningRecords (via Room)       │
                                       │
                    Query Filters Applied Automatically
                    WHERE HostelId = @HostelId AND IsDeleted = false
```

### Data Isolation Implementation

```csharp
// Global Query Filter in DbContext
modelBuilder.Entity<Hostel>(entity =>
{
    entity.HasQueryFilter(e => !e.IsDeleted);
});

// Result: All queries automatically exclude deleted records
// Example:
var hostels = await context.Hostels.ToListAsync();
// Generated SQL: SELECT * FROM Hostels WHERE IsDeleted = 0
```

---

## Entity Relationship Diagram (ERD)

```
┌─────────────────┐
│    Hostel       │
├─────────────────┤
│ HostelId (PK)   │◄─┐
│ Name            │  │
│ Location        │  │
│ Capacity        │  │
│ IsDeleted       │  │
└─────────────────┘  │
     │   │ │ │ │     │
     │   │ │ │ └──┐  │
     │   │ │ └───┐│  │
     │   │ └────┐││  │
     │   └─────┐│││  │
     │         │││└──┼─────┐
     │         │││   │     │
     ▼         │││   │     │
┌─────────────┐││    │     │
│ ApplicationUser    │     │
├─────────────┤│     │     │
│ Id (PK)     │└─┐   │     │
│ UserName    │  │   │     │
│ Email       │  │   │     │
│ FullName    │  │   │     │
│ HostelId(FK)├──┘   │     │
│ IsDeleted   │      │     │
└─────────────┘      │     │
  │        │         │     │
  │        │ 1:0-1   │     │
  │        └────┐    │     │
  │ 1:0-1       │    │     │
  ├───┐         │    │     │
  │   │         │    │     │
  ▼   ▼         ▼    ▼     ▼
Student  Worker  ┌────────┐
                  │ Room   │
                  │ Hostel │
                  │ (FK)   │
                  └────────┘
                       │1:N
                       ├─ Student (FK)
                       └─ CleaningRecord (FK)
                               │
                               └─ Worker (FK)

Complaint ◄─ Student
Complaint ◄─ Hostel

Fee ◄─ Student
Fee ◄─ Hostel
```

---

## Authentication & Authorization Flow

```
┌─────────────────────────────────────────────────────────────┐
│                     User Login                              │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
         ┌─────────────────────────────────┐
         │  Validate Credentials           │
         │  (UserName/Email + Password)    │
         │  Against ApplicationUser Table  │
         └─────────────┬───────────────────┘
                       │
         ┌─────────────▼─────────────┐
         │  Credentials Valid?       │
         └──┬───────────────┬────────┘
            │ YES           │ NO
            ▼               ▼
        Generate        Return 401
        JWT Token       Unauthorized
            │
            ├─ Header: typ=JWT, alg=HS256
            ├─ Payload:
            │   {
            │     "sub": userId,
            │     "email": user@email.com,
            │     "roles": ["Admin", "Student"],
            │     "hostelId": 1,
            │     "iat": 1234567890,
            │     "exp": 1234654290
            │   }
            ├─ Signature: HMACSHA256(secret)
            │
            ▼
        Return Token to Client
            │
            ▼
        Client stores token
            │
            ├─ Subsequent requests include:
            │  Authorization: Bearer {token}
            │
            ▼
        ┌──────────────────────────────┐
        │  JWT Validation Middleware   │
        │  • Signature validation      │
        │  • Expiration check          │
        │  • Claims extraction         │
        └──────┬───────────────────────┘
               │
        ┌──────▼──────┐
        │ Valid Token?│
        └──┬──────┬───┘
           │ YES  │ NO
           ▼      ▼
        Extract Return 401
        Claims  Unauthorized
           │
           ▼
        ┌────────────────────────────┐
        │ Role-Based Authorization   │
        │ Check [Authorize(Roles)]   │
        │ Attributes                 │
        └──────┬─────────────────────┘
               │
        ┌──────▼──────┐
        │ Role Valid? │
        └──┬──────┬───┘
           │ YES  │ NO
           ▼      ▼
        Execute Return 403
        Request Forbidden
```

### Implemented Roles

```
┌──────────────────────────────────────────────────────┐
│                    Admin Role                        │
├──────────────────────────────────────────────────────┤
│ • Create/Update/Delete Hostels                       │
│ • Manage all Users                                   │
│ • Assign Users to Hostels                           │
│ • View all Reports & Analytics                      │
│ • Configure System Settings                         │
│ • Manage Fees & Payments                            │
└──────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────┐
│                   Student Role                       │
├──────────────────────────────────────────────────────┤
│ • View own Profile                                   │
│ • View Room Assignment                               │
│ • File Complaints                                    │
│ • View own Fees & Payment History                    │
│ • View Hostel Rules & Announcements                  │
│ • Cannot delete/modify other data                    │
└──────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────┐
│                   Worker Role                        │
├──────────────────────────────────────────────────────┤
│ • Update Cleaning Records                            │
│ • View assigned Rooms                                │
│ • Report Issues                                      │
│ • View Hostel Announcements                          │
│ • Cannot modify complaints or fees                   │
└──────────────────────────────────────────────────────┘
```

---

## Soft Delete & Data Lifecycle

```
┌────────────────────────────────────────────────────────┐
│              Entity Lifecycle States                   │
└────────────────────────────────────────────────────────┘

┌─────────────────┐
│    CREATE       │
│ (Insert Record) │
├─────────────────┤
│ IsDeleted = 0   │
│ CreatedAt = NOW │
│ UpdatedAt = NULL│
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│     ACTIVE      │
│ (In Use)        │
├─────────────────┤
│ Visible in all  │
│ queries         │
│ Can be updated  │
└────────┬────────┘
         │ Update Operation
         ▼
┌─────────────────┐
│   UPDATED       │
│ (Modified)      │
├─────────────────┤
│ IsDeleted = 0   │
│ UpdatedAt = NOW │
└────────┬────────┘
         │ Delete Operation
         ▼
┌─────────────────┐
│  SOFT DELETED   │
│ (Marked Deleted)│
├─────────────────┤
│ IsDeleted = 1   │
│ UpdatedAt = NOW │
│ Hidden in normal│
│ queries         │
│ Recoverable     │
└─────────────────┘

Query Behavior:
─────────────────

Standard Query:
  FROM Entities WHERE IsDeleted = 0
  (Only active records)

Include Deleted:
  FROM Entities
  (All records, with explicit handling)

Recovery:
  UPDATE Entities SET IsDeleted = 0
  WHERE Id = @Id
  (Soft delete is reversible)
```

---

## Caching Strategy

```
┌─────────────────────────────────────────────────────────┐
│          Redis Caching Architecture                     │
└─────────────────────────────────────────────────────────┘

Request Flow:
──────────────

1. Client Request Arrives
       │
       ▼
   ┌───────────┐
   │ Check     │
   │ Cache?    │
   └───┬───┬───┘
       │   │
     Hit │ │ Miss
        ▼ ▼
      Data Get from
       │  Database
       │  │
       │  ▼
       │ Set in Cache
       │  (TTL: 60 min)
       │  │
       └──┘
          │
          ▼
      Return Response

Cache Keys Structure:
──────────────────────

user:{userId}
  └─ Example: user:123
  └─ TTL: 1 hour
  └─ Data: ApplicationUser object

hostel:{hostelId}
  └─ Example: hostel:5
  └─ TTL: 2 hours
  └─ Data: Hostel object

room:{roomId}
  └─ Example: room:42
  └─ TTL: 1 hour
  └─ Data: Room object

student:{studentId}
  └─ Example: student:78
  └─ TTL: 1 hour
  └─ Data: Student object

complaint:{complaintId}
  └─ Example: complaint:15
  └─ TTL: 30 minutes
  └─ Data: Complaint object

Cache Invalidation:
──────────────────

Create:
  ├─ Set new cache entry
  └─ Invalidate list queries

Update:
  ├─ Remove old cache
  ├─ Set updated cache
  └─ Invalidate related queries

Delete:
  ├─ Remove from cache
  └─ Invalidate list queries
```

---

## Error Handling & Exception Flow

```
┌─────────────────────────────────────────────────────┐
│        Global Exception Handling Pipeline           │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
         ┌─────────────────────┐
         │ Unhandled Exception │
         │ Thrown in App       │
         └────────┬────────────┘
                  │
                  ▼
         ┌─────────────────────────┐
         │ Exception Middleware    │
         │ Catches Exception       │
         │ Logs with ILogger       │
         └────────┬────────────────┘
                  │
        ┌─────────▼─────────┐
        │ Exception Type?   │
        └───┬──┬──┬──┬──┬───┘
            │  │  │  │  │
    ┌───────┘  │  │  │  └──────┐
    │          │  │  │         │
    ▼          ▼  ▼  ▼         ▼
ArgumentNull  Argument  Unauthorized  KeyNotFound  Other
Exception     Exception AccessExc.    Exception    Exceptions

    400        400      401          404          500
    ▼          ▼        ▼            ▼            ▼

Return Standardized JSON Response:
──────────────────────────────────────

{
  "success": false,
  "message": "Error description",
  "errors": ["detailed error message"]
}

Status Codes:
─────────────
400 Bad Request      - Invalid input/arguments
401 Unauthorized     - Authentication failed
404 Not Found        - Resource not found
500 Internal Server  - Unhandled exceptions
    Error
```

---

## Database Migration Flow

```
┌──────────────────────────────────────────────────────┐
│        Automatic Migration on Startup                │
└──────────────────┬─────────────────────────────────┘
                   │
                   ▼
         ┌─────────────────────┐
         │ Application Starts  │
         │ Program.cs runs     │
         └────────┬────────────┘
                  │
                  ▼
         ┌──────────────────────┐
         │ Create Service Scope │
         │ Get DbContext        │
         └────────┬─────────────┘
                  │
                  ▼
         ┌──────────────────────┐
         │ context.Database     │
         │ .Migrate()           │
         └────────┬─────────────┘
                  │
        ┌─────────▼──────────┐
        │ Migrations Applied?│
        └──┬───────────────┬─┘
           │ YES           │ NO
           │               └─ Check __EFMigrationsHistory
           │                  Apply pending migrations
           ▼
      ┌────────────────┐
      │ Database Ready │
      └────────┬───────┘
               │
               ▼
    ┌──────────────────────┐
    │ Run DatabaseSeeder   │
    │ .SeedDatabaseAsync() │
    └────────┬─────────────┘
             │
    ┌────────▼──────────┐
    │ Check existing    │
    │ data? (if exists) │
    │ skip seeding      │
    └────┬───────────┬──┘
         │ YES       │ NO
         └─ Return   Seed defaults:
            │       • Roles
            │       • Admin User
            │       • Hostels
            │       • Rooms
            ▼       • Workers
         Skip       • Students
                    • Complaints
                    • Fees
                    • etc.
             │
             ▼
    ┌──────────────────┐
    │ Application Ready│
    └──────────────────┘
```

---

## Data Validation & Security Layers

```
┌─────────────────────────────────────────────────────┐
│         Multi-Layer Validation & Security           │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
         ┌──────────────────────┐
         │ Layer 1: Auth & Authz │
         │ • JWT Validation      │
         │ • Role Checking       │
         │ • Tenant Isolation    │
         └────────┬─────────────┘
                  │
                  ▼
         ┌──────────────────────┐
         │ Layer 2: Input        │
         │ Validation            │
         │ • Null checks         │
         │ • Type validation     │
         │ • Range checks        │
         │ • FluentValidation    │
         └────────┬─────────────┘
                  │
                  ▼
         ┌──────────────────────┐
         │ Layer 3: Business     │
         │ Logic                 │
         │ • Domain rules        │
         │ • State validation    │
         │ • Constraint checks   │
         └────────┬─────────────┘
                  │
                  ▼
         ┌──────────────────────┐
         │ Layer 4: Database     │
         │ Constraints           │
         │ • Foreign Keys        │
         │ • Unique constraints  │
         │ • Check constraints   │
         │ • Triggers (if used)  │
         └────────┬─────────────┘
                  │
                  ▼
    ┌──────────────────────────┐
    │ Data Persisted Safely    │
    │ & Securely               │
    └──────────────────────────┘
```

---

## Performance Optimization Points

```
┌─────────────────────────────────────────┐
│    Performance Optimization Strategy    │
└──────────────────┬──────────────────────┘
                   │
        ┌──────────┼──────────┐
        │          │          │
        ▼          ▼          ▼
    ┌─────────┐ ┌──────────┐ ┌──────────┐
    │Caching  │ │Database  │ │API Query │
    │         │ │Indexing  │ │Filtering │
    └────┬────┘ └────┬─────┘ └────┬─────┘
         │           │            │
         ├─ Redis    ├─ HostelId  ├─ Pagination
         │ caching   │ index      │
         │ for hot   ├─ Email     ├─ Select
         │ data      │ index      │ only needed
         │           │            │ fields
         │           ├─ Composite ├─ Use async
         │           │ indexes    │ queries
         │           │            │
         │           │            └─ Include
         │           │              related data
         │           │              only when
         │           │              needed
         │
         └─ Default TTL: 60 minutes
           Customizable per entity
```

---

## Deployment & Environment Configuration

```
Development Environment:
────────────────────────
• Local SQL Server / LocalDB
• No authentication required
• All logging enabled (Debug level)
• CORS: Allow All
• Redis: localhost:6379

Staging Environment:
────────────────────
• Staging SQL Server
• JWT validation enabled
• Logging: Information level
• CORS: Specific origins
• Redis: Staging server
• Database backups: Daily

Production Environment:
──────────────────────
• Production SQL Server (HA setup)
• All security measures enabled
• Logging: Warning/Error level
• CORS: Restricted origins only
• Redis: Production cluster
• Database backups: Hourly
• Secrets: Environment variables
• SSL/TLS: Enforced
```

---

## Summary

This architecture provides:

✅ **Scalability** - Multi-tenant support with data isolation  
✅ **Security** - JWT auth, role-based access control, soft deletes  
✅ **Performance** - Redis caching, database indexing  
✅ **Reliability** - Global exception handling, audit trails  
✅ **Maintainability** - Clean separation of concerns  
✅ **Extensibility** - Service-oriented design for future features
