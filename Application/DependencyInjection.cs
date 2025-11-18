using YoutubeApiSynchronize.Application.Services;
using YoutubeApiSynchronize.Application.UseCases;
using YoutubeApiSynchronize.Core.Interfaces;
using YoutubeApiSynchronize.Options;

namespace YoutubeApiSynchronize.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core Services
        services.AddScoped<IYouTubeService, YouTubeService>();
        services.AddScoped<INotificationService, NotificationService>();
        
        // Use Cases
        services.AddScoped<SynchronizeYouTubeVideosUseCase>();
        
        // Options Configuration
        services.Configure<YoutubeConfig>(configuration.GetSection(nameof(YoutubeConfig)));
        services.Configure<LogConfig>(configuration.GetSection(nameof(LogConfig)));
        services.Configure<ShortPlaylistsOptions>(configuration.GetSection(nameof(ShortPlaylistsOptions)));
        
        return services;
    }
}
