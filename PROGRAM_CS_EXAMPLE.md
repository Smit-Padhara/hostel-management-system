// Complete Program.cs Configuration Example
// Add this to your Program.cs file

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Extensions;
using SmartHostelManagementSystem.Middleware;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Interfaces;
using SmartHostelManagementSystem.Services.Implementations;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============= DATABASE CONFIGURATION =============
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============= IDENTITY CONFIGURATION =============
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ============= AUTHENTICATION & JWT =============
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] 
    ?? throw new InvalidOperationException("JWT SecretKey not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ============= REDIS CACHE CONFIGURATION =============
var redisConnection = builder.Configuration.GetConnectionString("Redis") 
    ?? "localhost:6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
});

// Register Cache Service
builder.Services.AddScoped<ICacheService, CacheService>();

// ============= APPLICATION SERVICES REGISTRATION =============
// This registers all 7 services:
// - IHostelService → HostelService
// - IRoomService → RoomService
// - IStudentService → StudentService
// - IComplaintService → ComplaintService
// - IFeeService → FeeService
// - ICleaningService → CleaningService
// - IDashboardService → DashboardService
builder.Services.AddApplicationServices();

// ============= AUTOMAPPER CONFIGURATION =============
builder.Services.AddAutoMapper(typeof(Program));

// ============= MVC & API CONFIGURATION =============
builder.Services.AddControllers();

// ============= SWAGGER/OPENAPI =============
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Smart Hostel Management System API",
        Version = "v1.0",
        Description = "Complete REST API for hostel management with 7 services and 6 controllers"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============= CORS CONFIGURATION =============
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });

    options.AddPolicy("AllowSpecific", builder =>
    {
        builder.WithOrigins("https://localhost:3000", "http://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// ============= LOGGING CONFIGURATION =============
builder.Services.AddLogging(options =>
{
    options.ClearProviders();
    options.AddConsole();
    options.AddDebug();
    if (builder.Environment.IsProduction())
        options.AddEventLog();
});

// ============= BUILD APP =============
var app = builder.Build();

// ============= MIDDLEWARE PIPELINE =============

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Hostel API V1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

// HTTPS Redirect
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll"); // or specific policy

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Custom Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Map Controllers
app.MapControllers();

// ============= BUILD & RUN =============
app.Run();


/*
 * ================================================================
 * ADDITIONAL CONFIGURATION NOTES
 * ================================================================
 * 
 * 1. appsettings.json must contain:
 * 
 *    {
 *      "ConnectionStrings": {
 *        "DefaultConnection": "Server=.;Database=HostelManagement;Trusted_Connection=true;",
 *        "Redis": "localhost:6379"
 *      },
 *      "Jwt": {
 *        "SecretKey": "your-very-long-secret-key-with-minimum-32-characters",
 *        "Issuer": "HostelApp",
 *        "Audience": "HostelAppUsers"
 *      },
 *      "Logging": {
 *        "LogLevel": {
 *          "Default": "Information"
 *        }
 *      }
 *    }
 * 
 * 2. Run migrations:
 *    dotnet ef migrations add InitialCreate
 *    dotnet ef database update
 * 
 * 3. Redis Setup:
 *    - Install Redis: https://redis.io/download
 *    - Or use Docker: docker run -d -p 6379:6379 redis:latest
 * 
 * 4. Available Endpoints (after startup):
 *    - Swagger UI: https://localhost:5001
 *    - API Health: https://localhost:5001/api/dashboard/health/check
 *    - All 50+ endpoints documented in Swagger
 * 
 * 5. Services Registered:
 *    ✓ HostelService - Hostel CRUD and statistics
 *    ✓ RoomService - Room management with capacity control
 *    ✓ StudentService - Student assignment with validation
 *    ✓ ComplaintService - Complaint tracking and resolution
 *    ✓ FeeService - Fee and payment management
 *    ✓ CleaningService - CORE MODULE for daily cleaning tasks
 *    ✓ DashboardService - Analytics and reporting
 * 
 * 6. Controllers Available:
 *    ✓ HostelController (api/hostel)
 *    ✓ RoomController (api/room)
 *    ✓ StudentController (api/student)
 *    ✓ ComplaintController (api/complaint)
 *    ✓ FeeController (api/fee)
 *    ✓ CleaningController (api/cleaning) - CORE
 *    ✓ DashboardController (api/dashboard)
 * 
 * ================================================================
 */
