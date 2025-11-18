using Microsoft.EntityFrameworkCore;
using Quartz;
using YoutubeApiSynchronize.Core.Interfaces;
using YoutubeApiSynchronize.Infrastructure.Database;
using YoutubeApiSynchronize.Infrastructure.ExternalServices;
using YoutubeApiSynchronize.Infrastructure.Repositories;
using YoutubeApiSynchronize.Options;

namespace YoutubeApiSynchronize.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Configuration
        services.AddDbContext<MedreseDbContext>(options =>
        {
            var db = configuration.GetSection("DB").Get<DatabaseSettings>()!;
            options.UseNpgsql(db.ConnectionString);
        });
        
        // External Services
        services.AddScoped<IYouTubeApiClient, YouTubeApiClient>();
        
        // Repositories
        services.AddScoped<IYouTubeRepository, YouTubeRepository>();
        
        return services;
    }
}

