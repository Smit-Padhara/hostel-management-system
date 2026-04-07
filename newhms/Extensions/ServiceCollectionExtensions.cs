using SmartHostelManagementSystem.Services.Implementations;
using SmartHostelManagementSystem.Services.Interfaces;

namespace SmartHostelManagementSystem.Extensions;

/// <summary>
/// Extension methods for configuring services in the dependency injection container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all application services
    /// Call this in Program.cs: services.AddApplicationServices();
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register service interfaces with their implementations
        services.AddScoped<IHostelService, HostelService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IComplaintService, ComplaintService>();
        services.AddScoped<IFeeService, FeeService>();
        services.AddScoped<ICleaningService, CleaningService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }

    /// <summary>
    /// Register AutoMapper for DTO mapping
    /// Call this in Program.cs: services.AddAutoMapper(typeof(Program));
    /// </summary>
    public static IServiceCollection AddApplicationAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        return services;
    }
}
