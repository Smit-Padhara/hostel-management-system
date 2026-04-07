# API Quick Reference Guide

## 🚀 Quick Start

### Register Services in Program.cs
```csharp
builder.Services.AddApplicationServices();
```

---

## 📋 All Endpoints Summary

### **Hostels** (`api/hostel`)
```bash
# Create
POST   /api/hostel
       Content: { "name": "Hostel A", "location": "City", "capacity": 100 }

# Read
GET    /api/hostel                    # All hostels
GET    /api/hostel/1                  # Get by ID
GET    /api/hostel/1/stats            # With statistics
PUT    /api/hostel/1                  # Update
DELETE /api/hostel/1                  # Delete
```

---

### **Rooms** (`api/room`)
```bash
# Create
POST   /api/room
       { "hostelId": 1, "roomNumber": "101", "capacity": 4 }

# List
GET    /api/room/hostel/1             # All rooms in hostel
GET    /api/room/available/1          # Available rooms (with capacity)

# Check Status
GET    /api/room/1/capacity           # Available seats
GET    /api/room/1/full               # Is full?

# Manage
PUT    /api/room/1                    # Update
DELETE /api/room/1                    # Delete
```

---

### **Students** (`api/student`)
```bash
# View
GET    /api/student/1                 # Get by ID
GET    /api/student/hostel/1          # All in hostel
GET    /api/student/room/5            # All in room
GET    /api/student/unassigned/1      # Unassigned students
GET    /api/student/count/1           # Total count
GET    /api/student/1/assigned        # Is assigned?

# Assignment (⚠️ WITH CAPACITY VALIDATION)
POST   /api/student/assign
       { "studentId": 5, "roomId": 3 }

POST   /api/student/5/unassign        # Unassign student
```

---

### **Complaints** (`api/complaint`)
```bash
# Create
POST   /api/complaint
       { "studentId": 5, "title": "Water issue", "description": "..." }

# Read
GET    /api/complaint/1               # By ID
GET    /api/complaint/hostel/1        # All in hostel
GET    /api/complaint/hostel/1/status/Pending
GET    /api/complaint/student/5       # Student's complaints
GET    /api/complaint/pending-count/1 # Count pending

# Manage
PUT    /api/complaint/1/status
       { "status": "Resolved", "resolution": "Fixed" }
DELETE /api/complaint/1               # Delete
```

---

### **Fees** (`api/fee`)
```bash
# Create
POST   /api/fee
       { "studentId": 5, "amount": 5000, "paymentStatus": "Pending" }

# Read
GET    /api/fee/1                     # By ID
GET    /api/fee/student/5             # Student's fees
GET    /api/fee/hostel/1              # All in hostel
GET    /api/fee/pending/1             # Pending fees
GET    /api/fee/overdue/1             # Overdue fees
GET    /api/fee/total-pending/1       # Total pending amount
GET    /api/fee/total-collected/1     # Total collected
GET    /api/fee/students-with-pending/1  # Count with pending

# Payment & Receipt
POST   /api/fee/1/payment?amount=2000&paymentDate=2024-01-15
GET    /api/fee/1/receipt?amount=2000&date=2024-01-15
```

---

### **Cleaning** (`api/cleaning`) ⭐ CORE MODULE
```bash
# Create
POST   /api/cleaning?roomId=5&date=2024-01-15

# Today's Tasks (CORE)
GET    /api/cleaning/today/1          # Today's cleaning tasks

# Pending Work (CORE)
GET    /api/cleaning/pending/1        # Pending records
GET    /api/cleaning/pending-count/1  # Count pending rooms

# Mark Complete
POST   /api/cleaning/mark-cleaned/5?workerId=2&remarks="Done"

# History & Reports (CORE)
GET    /api/cleaning/history/5        # 30 days history
GET    /api/cleaning/report/1?date=2024-01-15
GET    /api/cleaning/uncleaned/1?days=3

# Manage
PUT    /api/cleaning/1/status
       { "status": "Cleaned", "workerId": 2 }
DELETE /api/cleaning/1                # Delete
```

---

### **Dashboard** (`api/dashboard`)
```bash
# Summary (All Metrics)
GET    /api/dashboard/1

# Detailed Metrics
GET    /api/dashboard/1/utilization        # Room stats
GET    /api/dashboard/1/complaint-metrics  # Complaint stats
GET    /api/dashboard/1/fee-metrics        # Fee stats
GET    /api/dashboard/1/alerts             # Critical alerts

# Supporting
GET    /api/dashboard/1/recent-complaints?limit=5
POST   /api/dashboard/1/cache              # Pre-cache data
DELETE /api/dashboard/1/cache              # Clear cache
```

---

## 🔴 Critical Validations

### Room Over-Allocation Prevention
```
StudentService.AssignStudentToRoomAsync()
├─ Check room capacity
├─ Check if full
└─ THROW if room.occupancy >= room.capacity
```

### Student Assignment Rules
```
✓ Student exists
✓ Not already assigned
✓ Room exists
✓ Same hostel as room
✓ Room has available capacity ← CRITICAL
```

### Fee Payment Rules
```
✓ Amount > 0
✓ Amount ≤ remaining balance
✓ Updates status automatically (Pending → Partial → Paid)
```

---

## 📊 Cleaning Module (CORE)

### Requirement Checklist ✅
- ✅ Mark room as Cleaned / Pending → `PUT /cleaning/{id}/status`
- ✅ Get today's cleaning report → `GET /cleaning/today/{hostelId}`
- ✅ Get pending rooms → `GET /cleaning/pending/{hostelId}`
- ✅ Cleaning history → `GET /cleaning/history/{roomId}`
- ✅ Identify uncleaned rooms → `GET /cleaning/uncleaned/{hostelId}`

### Daily Workflow
```
1. Morning: GET /api/cleaning/today/1
   → View all tasks for the day

2. During day: POST /api/cleaning/mark-cleaned/5
   → Mark rooms as completed

3. Evening: GET /api/cleaning/pending/1
   → Check what still needs cleaning

4. Report: GET /api/cleaning/report/1
   → Generate daily report
```

---

## 💡 Common Patterns

### Get All Items (Pageable)
```
GET /api/{controller}?page=1&pageSize=10
```

### Filter by Date
```
GET /api/cleaning/history/5?days=30
GET /api/cleaning/report/1?date=2024-01-15
```

### Bulk Operations
```
POST /api/{resource}           # Create
PUT  /api/{resource}/{id}      # Update
DELETE /api/{resource}/{id}    # Delete
```

### Status Updates
```
PUT /api/complaint/1/status
    { "status": "Resolved", "resolution": "Fixed" }

PUT /api/cleaning/1/status
    { "status": "Cleaned", "workerId": 2 }
```

---

## 🎯 Response Format

### Success (200)
```json
{
  "roomId": 1,
  "roomNumber": "101",
  "capacity": 4,
  "currentOccupancy": 3,
  "availableSeats": 1
}
```

### Created (201)
```json
{
  "headers": {
    "location": "/api/hostel/5"
  },
  "body": { ...resource data... }
}
```

### Validation Error (400)
```json
{
  "message": "Room 101 is at full capacity (4/4). Cannot assign."
}
```

### Not Found (404)
```json
{
  "message": "Hostel with ID 999 not found"
}
```

---

## 🔒 Error Handling

All controllers return:
- ✅ 200 OK - Success
- ✅ 201 Created - Resource created
- ✅ 204 No Content - Deleted
- ✅ 400 Bad Request - Validation/Business Logic error
- ✅ 404 Not Found - Resource doesn't exist
- ✅ 500 Internal Server Error - Unexpected error

---

## ⚡ Performance Tips

1. **Use available endpoints** - Get only what you need
   ```
   GET /api/room/available/1     # Instead of all rooms
   ```

2. **Leverage caching** - Dashboard data cached for 5 min
   ```
   GET /api/dashboard/1          # Uses Redis
   ```

3. **Batch operations** - Create multiple records efficiently
   ```
   POST /api/fee (multiple)
   ```

---

## 🧪 Testing Checklist

- [ ] Create hostel, room, assign student
- [ ] Try assigning to full room → Should fail
- [ ] Create complaint, update status
- [ ] Record fee and payment → Check receipt
- [ ] Get today's cleaning tasks
- [ ] Mark rooms cleaned
- [ ] View cleaning report
- [ ] Check dashboard metrics
- [ ] Verify Redis caching works

---

## 📞 Support

For API documentation details, see `IMPLEMENTATION_GUIDE.md`

Services registered: 7
Controllers: 6
Total Endpoints: 50+
