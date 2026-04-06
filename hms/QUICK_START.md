# 🚀 Quick Setup Guide

## Prerequisites
- .NET 10.0 SDK
- SQL Server (LocalDB)
- Redis
- Visual Studio 2022 or VS Code

## Step 1: Restore NuGet Packages

```bash
cd hms
dotnet restore
```

## Step 2: Configure Database Connection

Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartHostelDB;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

## Step 3: Start Redis

```bash
redis-server
```

## Step 4: Run the Application

```bash
dotnet run
```

The API will be available at: `https://localhost:5001`

## Step 5: Access Swagger UI

Open browser and navigate to: `https://localhost:5001`

## Test Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@hms.com | Admin@123 |
| Student | student1@hms.com | Student@123 |
| Worker | worker1@hms.com | Worker@123 |

## Database Seeding

The database is automatically seeded on first run with:
- 3 Hostels
- 1 Admin User
- 3 Workers
- 4 Students
- 30 Rooms (10 per hostel)
- Sample Complaints, Fees, and Cleaning Records

## Troubleshooting

### Redis Connection Error
Ensure Redis is running on localhost:6379

### Database Connection Error
Check SQL Server is running and connection string is correct

### Migration Error
```bash
dotnet ef database drop --force
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

**Ready to test! 🎉**
