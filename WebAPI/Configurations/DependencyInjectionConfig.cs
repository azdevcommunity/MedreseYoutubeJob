using Quartz;
using YoutubeApiSynchronize.Application;
using YoutubeApiSynchronize.Infrastructure;
using YoutubeApiSynchronize.WebAPI.Jobs;

namespace YoutubeApiSynchronize.WebAPI.Configurations;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Infrastructure Services (Database, Repositories, External Services)
        services.AddInfrastructureServices(configuration);
        
        // Add Application Services (Business Logic, Use Cases)
        services.AddApplicationServices(configuration);
        
        // Add CORS Configuration
        services.AddCorsConfiguration(configuration);
        
        // Add Quartz Background Jobs
        services.AddQuartzConfiguration();
        
        return services;
    }
}

