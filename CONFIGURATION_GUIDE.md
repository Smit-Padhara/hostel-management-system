# ⚙️ Configuration & Environment Variables Guide

## Environment Setup

### Development Environment

#### appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartHostelDB;Trusted_Connection=true;TrustServerCertificate=true;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "SecretKey": "dev-secret-key-this-is-only-for-local-development-must-be-32-chars",
    "Issuer": "SmartHostelAPI",
    "Audience": "SmartHostelClient",
    "ExpiresInHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "SmartHostelManagementSystem": "Debug"
    }
  },
  "AllowedHosts": "*",
  "CORS": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]
  }
}
```

### Staging Environment

#### appsettings.Staging.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=staging-db-server;Database=SmartHostelDB;User Id=sa;Password=SecurePassword123!;Encrypt=true;TrustServerCertificate=false;",
    "Redis": "staging-redis:6379"
  },
  "Jwt": {
    "SecretKey": "staging-secret-key-minimum-32-characters-required-change-this",
    "Issuer": "SmartHostelAPI",
    "Audience": "SmartHostelClient",
    "ExpiresInHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "SmartHostelManagementSystem": "Information"
    }
  },
  "AllowedHosts": "staging-api.yourdomain.com",
  "CORS": {
    "AllowedOrigins": ["https://staging.yourdomain.com"]
  }
}
```

### Production Environment

#### appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db-server;Database=SmartHostelDB;User Id=sa;Password=${DB_PASSWORD};Encrypt=true;TrustServerCertificate=false;",
    "Redis": "prod-redis:6379"
  },
  "Jwt": {
    "SecretKey": "${JWT_SECRET}",
    "Issuer": "SmartHostelAPI",
    "Audience": "SmartHostelClient",
    "ExpiresInHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "SmartHostelManagementSystem": "Information"
    }
  },
  "AllowedHosts": "api.yourdomain.com",
  "CORS": {
    "AllowedOrigins": ["https://yourdomain.com"]
  }
}
```

---

## Environment Variables

### Required Environment Variables

```bash
# Database
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:5000

# Connection Strings (use in production, avoids appsettings.json secrets)
ConnectionStrings__DefaultConnection=Server=prod-server;Database=SmartHostelDB;User Id=sa;Password=SecurePassword123!;Encrypt=true;
ConnectionStrings__Redis=prod-redis:6379

# JWT Configuration
Jwt__SecretKey=your-production-secret-key-minimum-32-characters-long
Jwt__Issuer=SmartHostelAPI
Jwt__Audience=SmartHostelClient
Jwt__ExpiresInHours=24

# Logging
Logging__LogLevel__Default=Warning
Logging__LogLevel__Microsoft.AspNetCore=Warning

# CORS
CORS__AllowedOrigins__0=https://yourdomain.com
CORS__AllowedOrigins__1=https://app.yourdomain.com
```

### Docker Example

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0

COPY publish /app
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

ENTRYPOINT ["dotnet", "SmartHostelManagementSystem.dll"]
```

**Run Docker Container:**
```bash
docker run -p 5000:5000 \
  -e ConnectionStrings__DefaultConnection="Server=db;Database=SmartHostelDB;..." \
  -e Jwt__SecretKey="production-secret-key" \
  -e Jwt__Issuer="SmartHostelAPI" \
  smarthostelmgmt:latest
```

---

## Configuration Checklist

### Before First Run

- [ ] SQL Server instance is running
- [ ] Connection string is correct in appsettings.json
- [ ] JWT secret key is configured (min 32 characters)
- [ ] Redis is running (if using cache in development)
- [ ] .NET 10.0 SDK is installed

### Before Deployment

- [ ] Change JWT secret key to production value
- [ ] Update connection string to production database
- [ ] Update Redis connection string
- [ ] Configure CORS for production domain
- [ ] Disable AllowedHosts = "*"
- [ ] Set logging level to Warning/Error
- [ ] Use HTTPS in production
- [ ] Configure SSL certificate
- [ ] Set up database backups
- [ ] Configure monitoring and alerting

---

## Connection String Examples

### Local SQL Server (Windows Auth)
```
Server=(localdb)\mssqllocaldb;Database=SmartHostelDB;Trusted_Connection=true;TrustServerCertificate=true;
```

### Local SQL Server (SQL Auth)
```
Server=localhost;Database=SmartHostelDB;User Id=sa;Password=YourPassword123!;
```

### Azure SQL Database
```
Server=yourserver.database.windows.net;Database=SmartHostelDB;User Id=sqladmin@yourserver;Password=YourSecurePassword123!;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
```

### AWS RDS SQL Server
```
Server=myrds.c9akciq32.us-east-1.rds.amazonaws.com,1433;Database=SmartHostelDB;User Id=admin;Password=YourPassword123!;
```

### Docker SQL Server
```
Server=sqlserver;Database=SmartHostelDB;User Id=sa;Password=YourPassword123!;
```

---

## Redis Connection Examples

### Local Development
```
localhost:6379
```

### Docker
```
redis:6379
```

### AWS ElastiCache
```
myelasticache.abc123.ng.0001.use1.cache.amazonaws.com:6379
```

### Azure Cache for Redis
```
myredis.redis.cache.windows.net:6380,password=your-key,ssl=True
```

### Heroku Redis
```
redis-9876.heroku.com:12345
```

---

## Application Settings Reference

### Logging Configuration

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",                     // Default log level
    "Microsoft.AspNetCore": "Warning",            // ASP.NET Core logging
    "Microsoft.EntityFrameworkCore": "Warning",   // EF Core logging
    "Microsoft.EntityFrameworkCore.Database.Command": "Information",  // SQL queries
    "SmartHostelManagementSystem": "Debug"        // Application logging
  }
}
```

**Log Levels:**
- `Trace` - Very detailed, usually disabled in production
- `Debug` - Debug information, disabled in production
- `Information` - Informational messages
- `Warning` - Warning messages
- `Error` - Error messages
- `Critical` - Critical errors
- `None` - Disable logging

### JWT Configuration

```json
"Jwt": {
  "SecretKey": "minimum-32-character-secret-key-for-hs256",
  "Issuer": "SmartHostelAPI",           // Who creates the token
  "Audience": "SmartHostelClient",      // Who uses the token
  "ExpiresInHours": 24                  // Token lifetime in hours
}
```

**Secret Key Requirements:**
- Minimum 32 characters for HMACSHA256
- Use strong random characters
- Never commit to version control
- Use environment variables in production
- Different keys per environment

### CORS Configuration

```json
"CORS": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:5173",
    "https://yourdomain.com"
  ],
  "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "PATCH"],
  "AllowedHeaders": ["Content-Type", "Authorization"],
  "AllowCredentials": true,
  "MaxAge": 86400
}
```

---

## Connection String Best Practices

### Security

1. **Never commit secrets to Git**
   ```bash
   # Add to .gitignore
   appsettings.Development.json
   appsettings.Production.json
   ```

2. **Use Environment Variables**
   ```csharp
   var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
   var connectionString = $"Server=server;Password={password};";
   ```

3. **Use Azure Key Vault**
   ```csharp
   // In Program.cs
   var keyVaultUrl = new Uri("https://myvault.vault.azure.net/");
   builder.Configuration.AddAzureKeyVault(keyVaultUrl, new DefaultAzureCredential());
   ```

4. **Use AWS Secrets Manager**
   ```csharp
   // In Program.cs
   builder.Configuration.AddSecretsManager();
   ```

---

## Troubleshooting Configuration Issues

### Issue: Connection String Not Found
**Solution:**
```bash
# Verify appsettings.json exists
ls appsettings.json

# Check connection string
cat appsettings.json | grep ConnectionString

# Verify spelling: DefaultConnection
```

### Issue: JWT Secret Too Short
**Error:** `key must be 32 bytes or more for HS256`  
**Solution:**
```json
{
  "Jwt": {
    "SecretKey": "minimum-32-character-secret-key-must-be-at-least-32-chars-or-longer-like-this"
  }
}
```

### Issue: Redis Connection Failed
**Solution:**
```bash
# Check Redis is running
redis-cli ping
# Output: PONG

# Verify connection string
ConnectionStrings__Redis=localhost:6379

# Test connection from app
ping redishost:6379
```

### Issue: Database Connection Failed
**Solution:**
```bash
# Test SQL Server connection
sqlcmd -S "(localdb)\mssqllocaldb" -E

# Verify connection string format
Server=(localdb)\mssqllocaldb;Database=SmartHostelDB;Trusted_Connection=true;

# Check LocalDB instance is running
sqllocaldb info
```

---

## Performance Tuning

### Connection String Optimization

```
# Connection Pooling (recommended for production)
Server=server;Database=db;Max Pool Size=20;Min Pool Size=5;

# Connection Timeout
Server=server;Database=db;Connection Timeout=30;

# Command Timeout
Server=server;Database=db;Command Timeout=30;
```

### Redis Configuration

```json
{
  "Redis": "localhost:6379",
  "RedisConfig": {
    "AbortOnConnectFail": false,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "DefaultDatabase": 0
  }
}
```

---

## Multi-Environment Deployment

### Development
```bash
ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### Staging
```bash
ASPNETCORE_ENVIRONMENT=Staging
dotnet run
```

### Production
```bash
ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

---

## Configuration Validation

### Sample Validation Code
```csharp
// In Program.cs after configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must be configured and at least 32 characters long");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found");
}

var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (string.IsNullOrEmpty(redisConnection))
{
    logger.LogWarning("Redis connection not configured, caching disabled");
}
```

---

## Secrets Management

### Local Development (User Secrets)
```bash
# Initialize user secrets
dotnet user-secrets init

# Set secret
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key-here"

# List secrets
dotnet user-secrets list

# Remove secret
dotnet user-secrets remove "Jwt:SecretKey"

# Clear all secrets
dotnet user-secrets clear
```

### Production (Environment Variables)
```bash
# Linux/Mac
export Jwt__SecretKey="production-secret-key"
export ConnectionStrings__DefaultConnection="..."

# Windows (PowerShell)
$env:Jwt__SecretKey="production-secret-key"
$env:ConnectionStrings__DefaultConnection="..."

# Windows (CMD)
set Jwt__SecretKey=production-secret-key
set ConnectionStrings__DefaultConnection=...
```

---

## Health Check Configuration

### appsettings.json Extension
```json
{
  "HealthCheck": {
    "Database": true,
    "Redis": true,
    "Memory": true
  }
}
```

### Program.cs Extension
```csharp
// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddRedis("redis:6379");

// Map health check endpoint
app.MapHealthChecks("/health");
```

---

## Configuration Summary

✅ **Development**: Uses LocalDB, minimal security, all logging enabled  
✅ **Staging**: Production-like, real database, moderate logging  
✅ **Production**: Maximum security, minimal logging, environment variables  

**Always remember:**
- Keep secrets out of version control
- Use environment variables in production
- Test configuration before deployment
- Monitor and log configuration changes
- Rotate secrets regularly

---

**Last Updated**: April 2026  
**Version**: 1.0  
**Framework**: ASP.NET Core 10.0
