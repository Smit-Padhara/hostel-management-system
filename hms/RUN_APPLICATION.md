# 🚀 Running the Smart Hostel Management System

## ✅ Build Status: SUCCESS

The project has been successfully built and is ready to run!

```
Build succeeded with 1 warning(s)
Restore succeeded with 1 warning(s)
```

## Prerequisites

Before running the application, ensure you have:

1. **.NET 10.0 SDK** installed
   ```powershell
   dotnet --version
   ```

2. **SQL Server LocalDB** installed and running
   - Installed with Visual Studio or SQL Server Express
   - LocalDB starts automatically when first connection is attempted

3. **Redis Server** running (optional, but recommended for caching)
   - Install: `choco install redis` or `wsl --install ubuntu && wsl redis-server`
   - Start: `redis-server` or `wsl redis-server`

## Step 1: Start Redis (Optional)

If you have Redis installed:

```powershell
redis-server
```

If using WSL:
```powershell
wsl redis-server
```

If you don't have Redis installed, the application will still work but caching will be disabled.

## Step 2: Run the Application

```powershell
cd "c:\Users\md idrish\OneDrive\Desktop\HMS\hms"
dotnet run
```

## Expected Output

When the application starts, you should see output similar to:

```
Using launch settings from C:\Users\md idrish\OneDrive\Desktop\HMS\hms\Properties\launchSettings.json...
Building...
hms
hms net10.0
...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to exit
```

## Step 3: Access the Application

Once running, access the application at:

- **Home Page**: https://localhost:5001/
- **Swagger API Docs**: https://localhost:5001/swagger
- **API Base URL**: https://localhost:5001/api/v1/

## Database Setup

The application automatically:
1. ✅ Applies pending EF Core migrations
2. ✅ Creates the `SmartHostelDB` database in LocalDB
3. ✅ Seeds sample data (hostels, users, rooms, students, etc.)

**No manual database setup is required!**

## Sample Test Credentials

After the application starts, use these credentials to test:

### Admin User
- **Email**: admin@hms.com
- **Password**: Admin@123
- **Role**: Admin

### Student User
- **Email**: student@hms.com
- **Password**: Student@123
- **Role**: Student

### Worker User
- **Email**: worker@hms.com
- **Password**: Worker@123
- **Role**: Worker

## Testing with cURL

### 1. Login to Get JWT Token

```powershell
$loginBody = @{
    email = "admin@hms.com"
    password = "Admin@123"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "https://localhost:5001/api/v1/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $loginBody `
    -SkipCertificateCheck

$token = ($response.Content | ConvertFrom-Json).data.token
Write-Host "JWT Token: $token"
```

### 2. Get All Hostels (requires authentication)

```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}

Invoke-WebRequest -Uri "https://localhost:5001/api/v1/hostel" `
    -Method GET `
    -Headers $headers `
    -SkipCertificateCheck | ConvertFrom-Json | ConvertTo-Json -Depth 5
```

### 3. Get Dashboard Statistics (Admin only)

```powershell
Invoke-WebRequest -Uri "https://localhost:5001/api/v1/dashboard/admin" `
    -Method GET `
    -Headers $headers `
    -SkipCertificateCheck | ConvertFrom-Json | ConvertTo-Json -Depth 5
```

## Swagger Testing

1. Open https://localhost:5001/swagger in your browser
2. Click the "Authorize" button (top right)
3. Enter your JWT token: `Bearer <your-token-here>`
4. Click "Authorize"
5. Now you can test all endpoints directly from Swagger UI

## Troubleshooting

### Issue: "SSL/TLS certificate validation failed"

**Solution**: The application uses a self-signed certificate for development.

Add this to PowerShell commands:
```powershell
-SkipCertificateCheck
```

Or use http instead of https (not recommended for production).

### Issue: "Could not connect to Redis server at localhost:6379"

**Solution**: Redis is optional. If you don't have it installed:
1. The application will still work
2. Caching will be disabled
3. Performance will be slightly lower

To install Redis:
```powershell
# Using Chocolatey
choco install redis

# Or using WSL on Windows
wsl --install ubuntu
wsl redis-server
```

### Issue: "Database connection failed"

**Solution**: Ensure SQL Server LocalDB is running.

Check LocalDB:
```powershell
sqllocaldb info
sqllocaldb start mssqllocaldb
```

Or verify connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartHostelDB;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

### Issue: "Port 5001 already in use"

**Solution**: Change the port in `Properties\launchSettings.json` or kill the existing process:

```powershell
# Find process using port 5001
netstat -ano | findstr :5001

# Kill the process (replace PID with the actual process ID)
taskkill /PID <PID> /F
```

## API Endpoints Overview

### Authentication
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - Login and get JWT token

### Hostel Management
- `GET /api/v1/hostel` - Get all hostels
- `GET /api/v1/hostel/{id}` - Get hostel by ID
- `GET /api/v1/hostel/{id}/stats` - Get hostel statistics
- `POST /api/v1/hostel` - Create new hostel (Admin only)
- `PUT /api/v1/hostel/{id}` - Update hostel (Admin only)
- `DELETE /api/v1/hostel/{id}` - Delete hostel (Admin only)

### Room Management
- `GET /api/v1/room` - Get all rooms
- `GET /api/v1/room/available` - Get available rooms
- `POST /api/v1/room` - Create new room

### Student Management
- `GET /api/v1/student` - Get all students
- `POST /api/v1/student` - Create student
- `POST /api/v1/student/{id}/allocate-room` - Allocate room to student

### Complaint Management
- `GET /api/v1/complaint` - Get all complaints (Admin)
- `POST /api/v1/complaint` - Create complaint (Student)
- `PUT /api/v1/complaint/{id}` - Update complaint status (Admin)

### Fee Management
- `GET /api/v1/fee` - Get all fees
- `POST /api/v1/fee` - Create fee (Admin)
- `POST /api/v1/fee/{id}/mark-as-paid` - Mark fee as paid
- `GET /api/v1/fee/{id}/receipt` - Get fee receipt

### Cleaning Management
- `GET /api/v1/cleaning/today-report` - Get today's cleaning report
- `POST /api/v1/cleaning/mark-cleaned` - Mark room as cleaned (Worker)
- `GET /api/v1/cleaning/pending` - Get pending cleaning tasks

### Dashboard
- `GET /api/v1/dashboard/admin` - Get admin dashboard (Admin only)
- `GET /api/v1/dashboard/hostel-stats` - Get hostel statistics

## Stopping the Application

Press `Ctrl+C` in the terminal to stop the application gracefully.

---

**Version**: 1.0  
**Last Updated**: April 6, 2026  
**Status**: ✅ Ready to Run
