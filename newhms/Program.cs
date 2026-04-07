using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartHostelManagementSystem.Data;
using SmartHostelManagementSystem.Middleware;
using SmartHostelManagementSystem.Models.Entities;
using SmartHostelManagementSystem.Services.Implementations;
using SmartHostelManagementSystem.Services.Interfaces;
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
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

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

builder.Services.AddAuthorization();

// ============= CORS CONFIGURATION =============
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ============= REDIS CACHING CONFIGURATION =============
builder.Services.AddStackExchangeRedisCache(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.Configuration = connectionString;
});

// ============= SERVICE REGISTRATION =============
builder.Services.AddScoped<ICacheService, CacheService>();

// ============= LOGGING CONFIGURATION =============
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// ============= API DOCUMENTATION (SWAGGER) =============
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============= CONTROLLERS =============
builder.Services.AddControllers();

var app = builder.Build();

// ============= DATABASE MIGRATION & SEEDING =============
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
    
    try
    {
        // Apply migrations
        context.Database.Migrate();
        logger.LogInformation("Database migration applied successfully");
        
        // Seed data
        await DatabaseSeeder.SeedDatabaseAsync(context, userManager, roleManager, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred during database migration or seeding");
    }
}

// ============= HTTP REQUEST PIPELINE =============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Hostel Management System API v1");
        options.RoutePrefix = string.Empty;
    });
}

// ============= MIDDLEWARE =============
app.UseExceptionMiddleware();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ============= HELLO WORLD ENDPOINT =============
app.MapGet("/", () => new
{
    message = "Welcome to Smart Hostel Management System API",
    version = "1.0",
    documentation = "/swagger",
    author = "Development Team"
}).WithName("HelloWorld");

app.Run();
