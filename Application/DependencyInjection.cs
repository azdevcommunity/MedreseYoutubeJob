using YoutubeApiSynchronize.Application.Services;
using YoutubeApiSynchronize.Application.Services.Article;
using YoutubeApiSynchronize.Application.Services.Author;
using YoutubeApiSynchronize.Application.Services.Category;
using YoutubeApiSynchronize.Application.Services.ContactUs;
using YoutubeApiSynchronize.Application.Services.File;
using YoutubeApiSynchronize.Application.Services.Notification;
using YoutubeApiSynchronize.Application.Services.Playlist;
using YoutubeApiSynchronize.Application.Services.Question;
using YoutubeApiSynchronize.Application.Services.Search;
using YoutubeApiSynchronize.Application.Services.Statistic;
using YoutubeApiSynchronize.Application.Services.Tag;
using YoutubeApiSynchronize.Application.Services.Video;
using YoutubeApiSynchronize.Application.Services.Youtube;
using YoutubeApiSynchronize.Application.UseCases;
using YoutubeApiSynchronize.Core.Interfaces;
using YoutubeApiSynchronize.Core.Interfaces.Article;
using YoutubeApiSynchronize.Core.Interfaces.File;
using YoutubeApiSynchronize.Core.Interfaces.Notification;
using YoutubeApiSynchronize.Core.Interfaces.Playlist;
using YoutubeApiSynchronize.Core.Interfaces.Question;
using YoutubeApiSynchronize.Core.Interfaces.Search;
using YoutubeApiSynchronize.Core.Interfaces.Statistic;
using YoutubeApiSynchronize.Core.Interfaces.Video;
using YoutubeApiSynchronize.Core.Interfaces.Youtube;
using YoutubeApiSynchronize.Core.Options;

namespace YoutubeApiSynchronize.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core Services
        services.AddScoped<IYouTubeService, YouTubeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileService, FileService>();
        
        // Feature Services
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IContactUsService, ContactUsService>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IStatisticService, StatisticService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IVideoService, VideoService>();
        services.AddScoped<IPlaylistService, PlaylistService>();
        
        // Use Cases
        services.AddScoped<SynchronizeYouTubeVideosUseCase>();
        
        // Options Configuration
        services.Configure<YoutubeConfig>(configuration.GetSection(nameof(YoutubeConfig)));
        services.Configure<LogConfig>(configuration.GetSection(nameof(LogConfig)));
        services.Configure<ShortPlaylistsOptions>(configuration.GetSection(nameof(ShortPlaylistsOptions)));
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.Key));
        
        return services;
    }
}
