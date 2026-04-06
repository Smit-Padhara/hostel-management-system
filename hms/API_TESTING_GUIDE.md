# 📖 API Testing Guide - Smart Hostel Management System

This document provides comprehensive examples for testing all API endpoints.

## 🔑 Authentication Flow

### 1. Register New User

**Endpoint**: `POST /api/v1/auth/register`

**Request Body - Student Registration**:
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "phoneNumber": "9876543210",
  "password": "Password@123",
  "role": "Student",
  "hostelId": 1,
  "rollNumber": "ENG005"
}
```

**Request Body - Admin Registration**:
```json
{
  "name": "Admin Officer",
  "email": "admin2@hms.com",
  "phoneNumber": "9123456789",
  "password": "Admin@456",
  "role": "Admin",
  "hostelId": 1
}
```

**Response**:
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": 5,
    "name": "John Doe",
    "email": "john@example.com",
    "phoneNumber": "9876543210",
    "role": "Student",
    "hostelId": 1,
    "createdAt": "2026-04-06T10:30:00Z"
  }
}
```

### 2. Login and Get JWT Token

**Endpoint**: `POST /api/v1/auth/login`

**Request Body**:
```json
{
  "email": "admin@hms.com",
  "password": "Admin@123"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOiIxIiwiRW1haWwiOiJhZG1pbkBobXMuY29tIiwiUm9sZSI6IkFkbWluIiwiSG9zdGVsSWQiOiIxIiwiZXhwIjoxNzc0NzE3NjAwfQ.3f...",
    "user": {
      "userId": 1,
      "name": "Admin User",
      "email": "admin@hms.com",
      "phoneNumber": "9876543210",
      "role": "Admin",
      "hostelId": 1,
      "createdAt": "2026-04-06T00:00:00Z"
    }
  }
}
```

**Use the token in subsequent requests**:
```bash
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🏢 Hostel Management Endpoints

### Get All Hostels

**Endpoint**: `GET /api/v1/hostel`  
**Authorization**: Admin only

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 3 hostels",
  "data": [
    {
      "hostelId": 1,
      "name": "Boys Hostel A",
      "location": "Block A, Campus",
      "description": "Main hostel for boys",
      "capacity": 200,
      "totalRooms": 10,
      "totalStudents": 20,
      "createdAt": "2026-04-06T00:00:00Z"
    },
    {
      "hostelId": 2,
      "name": "Girls Hostel B",
      "location": "Block B, Campus",
      "description": "Main hostel for girls",
      "capacity": 180,
      "totalRooms": 10,
      "totalStudents": 15,
      "createdAt": "2026-04-06T00:00:00Z"
    }
  ]
}
```

### Get Hostel Details

**Endpoint**: `GET /api/v1/hostel/1`

**Response**:
```json
{
  "success": true,
  "message": "Hostel retrieved successfully",
  "data": {
    "hostelId": 1,
    "name": "Boys Hostel A",
    "location": "Block A, Campus",
    "description": "Main hostel for boys",
    "capacity": 200,
    "totalRooms": 10,
    "totalStudents": 20,
    "createdAt": "2026-04-06T00:00:00Z"
  }
}
```

### Get Hostel Statistics

**Endpoint**: `GET /api/v1/hostel/1/stats`

**Response**:
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

### Create New Hostel

**Endpoint**: `POST /api/v1/hostel`  
**Authorization**: Admin only

**Request Body**:
```json
{
  "name": "Junior Hostel D",
  "location": "Block D, Campus",
  "description": "Hostel for junior students",
  "capacity": 150
}
```

**Response**:
```json
{
  "success": true,
  "message": "Hostel created successfully",
  "data": {
    "hostelId": 4,
    "name": "Junior Hostel D",
    "location": "Block D, Campus",
    "description": "Hostel for junior students",
    "capacity": 150,
    "totalRooms": 0,
    "totalStudents": 0,
    "createdAt": "2026-04-06T10:45:00Z"
  }
}
```

---

## 🛏️ Room Management Endpoints

### Get All Rooms

**Endpoint**: `GET /api/v1/room`

**Query Parameters**:
- `hostelId` (optional): Filter by hostel

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 10 rooms",
  "data": [
    {
      "roomId": 1,
      "roomNumber": "101",
      "capacity": 2,
      "currentOccupancy": 2,
      "hostelId": 1,
      "createdAt": "2026-04-06T00:00:00Z"
    },
    {
      "roomId": 2,
      "roomNumber": "102",
      "capacity": 4,
      "currentOccupancy": 3,
      "hostelId": 1,
      "createdAt": "2026-04-06T00:00:00Z"
    }
  ]
}
```

### Get Available Rooms

**Endpoint**: `GET /api/v1/room/available/list`

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 2 available rooms",
  "data": [
    {
      "roomId": 3,
      "roomNumber": "103",
      "availableSpots": 2
    },
    {
      "roomId": 5,
      "roomNumber": "105",
      "availableSpots": 1
    }
  ]
}
```

### Create Room

**Endpoint**: `POST /api/v1/room`  
**Authorization**: Admin only

**Request Body**:
```json
{
  "roomNumber": "301",
  "capacity": 4,
  "hostelId": 1
}
```

**Response**:
```json
{
  "success": true,
  "message": "Room created successfully",
  "data": {
    "roomId": 31,
    "roomNumber": "301",
    "capacity": 4,
    "currentOccupancy": 0,
    "hostelId": 1,
    "createdAt": "2026-04-06T10:50:00Z"
  }
}
```

---

## 👨‍🎓 Student Management Endpoints

### Get All Students

**Endpoint**: `GET /api/v1/student`  
**Authorization**: Admin only

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 4 students",
  "data": [
    {
      "studentId": 1,
      "userId": 5,
      "studentName": "Arjun Singh",
      "rollNumber": "ENG001",
      "roomId": 1,
      "roomNumber": "101",
      "hostelId": 1,
      "admissionDate": "2025-04-06T00:00:00Z",
      "createdAt": "2026-04-06T00:00:00Z"
    }
  ]
}
```

### Allocate Room to Student

**Endpoint**: `POST /api/v1/student/{studentId}/allocate-room`  
**Authorization**: Admin only

**Request Body**:
```json
{
  "studentId": 1,
  "roomId": 3
}
```

**Response**:
```json
{
  "success": true,
  "message": "Room allocated successfully",
  "data": {
    "studentId": 1,
    "userId": 5,
    "studentName": "Arjun Singh",
    "rollNumber": "ENG001",
    "roomId": 3,
    "roomNumber": "103",
    "hostelId": 1,
    "admissionDate": "2025-04-06T00:00:00Z",
    "createdAt": "2026-04-06T00:00:00Z"
  }
}
```

### Get Students in a Room

**Endpoint**: `GET /api/v1/student/room/1`

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 2 students",
  "data": [
    {
      "studentId": 1,
      "userId": 5,
      "studentName": "Arjun Singh",
      "rollNumber": "ENG001",
      "roomId": 1,
      "roomNumber": "101",
      "hostelId": 1,
      "admissionDate": "2025-04-06T00:00:00Z",
      "createdAt": "2026-04-06T00:00:00Z"
    }
  ]
}
```

---

## 📝 Complaint Management Endpoints

### Create Complaint

**Endpoint**: `POST /api/v1/complaint?studentId=1`  
**Authorization**: Student only

**Request Body**:
```json
{
  "title": "Room has water leakage",
  "description": "There is water leaking from the ceiling in the room. Please repair urgently."
}
```

**Response**:
```json
{
  "success": true,
  "message": "Complaint created successfully",
  "data": {
    "complaintId": 3,
    "studentId": 1,
    "studentName": "Arjun Singh",
    "title": "Room has water leakage",
    "description": "There is water leaking from the ceiling in the room. Please repair urgently.",
    "status": "Pending",
    "resolution": null,
    "createdAt": "2026-04-06T11:00:00Z",
    "resolvedAt": null
  }
}
```

### Get All Complaints

**Endpoint**: `GET /api/v1/complaint`  
**Authorization**: Admin only

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 3 complaints",
  "data": [
    {
      "complaintId": 1,
      "studentId": 1,
      "studentName": "Arjun Singh",
      "title": "Water leakage in room",
      "description": "There is a water leakage from the ceiling in the room. Please repair it as soon as possible.",
      "status": "Pending",
      "resolution": null,
      "createdAt": "2026-04-04T00:00:00Z",
      "resolvedAt": null
    }
  ]
}
```

### Get Complaints by Status

**Endpoint**: `GET /api/v1/complaint/status/Pending`  
**Authorization**: Admin only

**Response**: Same as above, filtered by status

### Update Complaint

**Endpoint**: `PUT /api/v1/complaint/1`  
**Authorization**: Admin only

**Request Body**:
```json
{
  "status": "Resolved",
  "resolution": "Ceiling leak has been repaired and room inspected."
}
```

**Response**:
```json
{
  "success": true,
  "message": "Complaint updated successfully",
  "data": {
    "complaintId": 1,
    "studentId": 1,
    "studentName": "Arjun Singh",
    "title": "Water leakage in room",
    "description": "There is a water leakage from the ceiling in the room. Please repair it as soon as possible.",
    "status": "Resolved",
    "resolution": "Ceiling leak has been repaired and room inspected.",
    "createdAt": "2026-04-04T00:00:00Z",
    "resolvedAt": "2026-04-06T11:05:00Z"
  }
}
```

---

## 💰 Fee Management Endpoints

### Create Fee

**Endpoint**: `POST /api/v1/fee`  
**Authorization**: Admin only

**Request Body**:
```json
{
  "studentId": 1,
  "amount": 5000.00,
  "dueDate": "2026-05-06"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Fee created successfully",
  "data": {
    "feeId": 5,
    "studentId": 1,
    "studentName": "Arjun Singh",
    "amount": 5000.00,
    "status": "Pending",
    "dueDate": "2026-05-06T00:00:00Z",
    "paidDate": null,
    "transactionId": null,
    "createdAt": "2026-04-06T11:10:00Z"
  }
}
```

### Get Fees by Student

**Endpoint**: `GET /api/v1/fee/student/1`

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 2 fees",
  "data": [
    {
      "feeId": 1,
      "studentId": 1,
      "studentName": "Arjun Singh",
      "amount": 5000.00,
      "status": "Pending",
      "dueDate": "2026-05-06T00:00:00Z",
      "paidDate": null,
      "transactionId": null,
      "createdAt": "2026-04-06T00:00:00Z"
    }
  ]
}
```

### Mark Fee as Paid

**Endpoint**: `POST /api/v1/fee/1/pay`

**Request Body**:
```json
{
  "transactionId": "TXN20260406001"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Fee marked as paid successfully",
  "data": {
    "feeId": 1,
    "studentId": 1,
    "studentName": "Arjun Singh",
    "amount": 5000.00,
    "status": "Paid",
    "dueDate": "2026-05-06T00:00:00Z",
    "paidDate": "2026-04-06T11:15:00Z",
    "transactionId": "TXN20260406001",
    "createdAt": "2026-04-06T00:00:00Z"
  }
}
```

### Get Fee Receipt

**Endpoint**: `GET /api/v1/fee/1/receipt`

**Response**:
```json
{
  "success": true,
  "message": "Receipt retrieved successfully",
  "data": {
    "feeId": 1,
    "studentId": 1,
    "studentName": "Arjun Singh",
    "hostelName": "Boys Hostel A",
    "amount": 5000.00,
    "paidDate": "2026-04-06T11:15:00Z",
    "transactionId": "TXN20260406001"
  }
}
```

---

## 🧹 Cleaning Management Endpoints

### Mark Room as Cleaned

**Endpoint**: `POST /api/v1/cleaning/mark`  
**Authorization**: Worker only

**Request Body**:
```json
{
  "roomId": 1,
  "status": "Cleaned",
  "remarks": "Room cleaned and sanitized"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Room cleaning status updated successfully",
  "data": {
    "recordId": 15,
    "roomId": 1,
    "roomNumber": "101",
    "workerId": 1,
    "workerName": "Rajesh Kumar",
    "date": "2026-04-06T11:20:00Z",
    "status": "Cleaned",
    "remarks": "Room cleaned and sanitized",
    "cleanedAt": "2026-04-06T11:20:00Z"
  }
}
```

### Get Today's Cleaning Report

**Endpoint**: `GET /api/v1/cleaning/today`

**Response**:
```json
{
  "success": true,
  "message": "Today's cleaning report retrieved successfully",
  "data": {
    "date": "2026-04-06T00:00:00Z",
    "totalRooms": 30,
    "cleanedRooms": 25,
    "pendingRooms": 5,
    "cleaningPercentage": 83.33,
    "records": [
      {
        "recordId": 15,
        "roomId": 1,
        "roomNumber": "101",
        "workerId": 1,
        "workerName": "Rajesh Kumar",
        "date": "2026-04-06T11:20:00Z",
        "status": "Cleaned",
        "remarks": "Room cleaned and sanitized",
        "cleanedAt": "2026-04-06T11:20:00Z"
      }
    ]
  }
}
```

### Get Pending Cleaning

**Endpoint**: `GET /api/v1/cleaning/pending`

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 5 pending cleaning tasks",
  "data": [
    {
      "roomId": 6,
      "roomNumber": "106",
      "lastCleanedAt": "2026-04-05T10:00:00Z",
      "assignedWorkerId": null,
      "assignedWorkerName": null
    }
  ]
}
```

### Get Cleaning History for Room

**Endpoint**: `GET /api/v1/cleaning/history/room/1?days=30`

**Response**:
```json
{
  "success": true,
  "message": "Retrieved 28 cleaning records",
  "data": [
    {
      "recordId": 15,
      "roomId": 1,
      "roomNumber": "101",
      "workerId": 1,
      "workerName": "Rajesh Kumar",
      "date": "2026-04-06T11:20:00Z",
      "status": "Cleaned",
      "remarks": "Room cleaned and sanitized",
      "cleanedAt": "2026-04-06T11:20:00Z"
    }
  ]
}
```

### Get Cleaning Records by Date Range

**Endpoint**: `GET /api/v1/cleaning/date-range?startDate=2026-04-01&endDate=2026-04-06`  
**Authorization**: Admin only

**Response**: Same as above

---

## 📊 Dashboard Endpoints

### Get Admin Dashboard

**Endpoint**: `GET /api/v1/dashboard/admin`  
**Authorization**: Admin only

**Response**:
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
    "hostelStats": [
      {
        "hostelId": 1,
        "hostelName": "Boys Hostel A",
        "totalRooms": 10,
        "occupiedRooms": 8,
        "totalStudents": 20,
        "totalComplaints": 3,
        "pendingComplaints": 1,
        "totalFeesPending": 50000.00
      }
    ]
  }
}
```

### Get Hostel Quick Stats

**Endpoint**: `GET /api/v1/dashboard/hostel/stats`

**Response**:
```json
{
  "success": true,
  "message": "Statistics retrieved successfully",
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

---

## ⚠️ Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Failed to create room",
  "errors": ["Room number already exists in this hostel"]
}
```

### 401 Unauthorized
```json
{
  "success": false,
  "message": "Unauthorized access"
}
```

### 403 Forbidden
```json
{
  "success": false,
  "message": "Access denied - insufficient permissions"
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Hostel not found"
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "An error occurred processing your request",
  "errors": ["Database connection failed"]
}
```

---

## 🧪 Test Scenarios

### Scenario 1: Complete Student Workflow

1. Register as student
2. Login to get token
3. Create complaint
4. Pay fee
5. Get fee receipt

### Scenario 2: Admin Workflow

1. Login as admin
2. Get all hostels
3. Get hostel statistics
4. Create room
5. Allocate student to room
6. View cleaning report
7. View dashboard

### Scenario 3: Worker Workflow

1. Login as worker
2. Get pending cleaning
3. Mark rooms as cleaned
4. Get today's cleaning report

---

**Last Updated**: April 2026
