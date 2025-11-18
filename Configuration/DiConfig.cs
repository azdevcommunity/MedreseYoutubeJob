using YoutubeApiSynchronize.Attributes;
using YoutubeApiSynchronize.Options;
using YoutubeApiSynchronize.Services;

namespace YoutubeApiSynchronize.Configuration;

[Configuration]
public class DiConfig
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<YoutubeService>();
        builder.Services.AddScoped<PubSubService>();
        builder.Services.Configure<YoutubeConfig>(builder.Configuration.GetSection(nameof(YoutubeConfig)));
        builder.Services.Configure<LogConfig>(builder.Configuration.GetSection(nameof(LogConfig)));
        builder.Services.Configure<ShortPlaylistsOptions>
            (builder.Configuration.GetSection(nameof(ShortPlaylistsOptions)));
    }
}