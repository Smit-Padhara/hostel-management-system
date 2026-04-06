using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Middleware;
using SmartHostelManagementSystem.Services.Implementations;
using SmartHostelManagementSystem.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============= SERVICE REGISTRATION =============

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction => 
        sqlServerOptionsAction.MigrationsAssembly("SmartHostelManagementSystem")));

// Redis Caching Configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "");

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

// Authorization
builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Service Layer Registration
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHostelService, HostelService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IFeeService, FeeService>();
builder.Services.AddScoped<ICleaningService, CleaningService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICacheService, CacheService>();

// API Versioning - Using routing instead
// (Removed - versioning handled via route prefix /api/v1)

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// ============= APPLICATION CONFIGURATION =============

var app = builder.Build();

// Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Apply migrations
    dbContext.Database.Migrate();
    
    // Seed data
    await DatabaseSeeder.SeedDatabaseAsync(dbContext);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Hostel Management System API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Middleware Pipeline
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Hello World endpoint for testing
app.MapGet("/", () => new
{
    message = "Welcome to Smart Hostel Management System API v1.0",
    version = "1.0",
    documentation = "/swagger",
    endpoints = new
    {
        auth = "/api/v1/auth",
        hostel = "/api/v1/hostel",
        room = "/api/v1/room",
        student = "/api/v1/student",
        complaint = "/api/v1/complaint",
        fee = "/api/v1/fee",
        cleaning = "/api/v1/cleaning",
        dashboard = "/api/v1/dashboard"
    }
})
.WithName("RootEndpoint");

app.Run();
