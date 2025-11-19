using Microsoft.EntityFrameworkCore;
using Quartz;
using YoutubeApiSynchronize.Core.Interfaces;
using YoutubeApiSynchronize.Core.Interfaces.Article;
using YoutubeApiSynchronize.Core.Interfaces.Author;
using YoutubeApiSynchronize.Core.Interfaces.Category;
using YoutubeApiSynchronize.Core.Interfaces.ContactUs;
using YoutubeApiSynchronize.Core.Interfaces.Playlist;
using YoutubeApiSynchronize.Core.Interfaces.Question;
using YoutubeApiSynchronize.Core.Interfaces.Search;
using YoutubeApiSynchronize.Core.Interfaces.Statistic;
using YoutubeApiSynchronize.Core.Interfaces.Tag;
using YoutubeApiSynchronize.Core.Interfaces.Video;
using YoutubeApiSynchronize.Core.Interfaces.Youtube;
using YoutubeApiSynchronize.Infrastructure.ExternalServices;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;
using YoutubeApiSynchronize.Infrastructure.Persistence.Options;
using YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

namespace YoutubeApiSynchronize.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        
        // Database Configuration
        services.AddDbContext<MedreseDbContext>(options =>
        {
            var db = configuration.GetSection(DatabaseSettings.Key).Get<DatabaseSettings>()!;

            options.UseNpgsql(db.ConnectionString);
        });
        
        // External Services
        services.AddScoped<IYouTubeApiClient, YouTubeApiClient>();
        
        // Repositories
        services.AddScoped<IYouTubeRepository, YouTubeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IContactUsRepository, ContactUsRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IStatisticRepository, StatisticRepository>();
        services.AddScoped<ISearchRepository, SearchRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();
        
        return services;
    }
}

