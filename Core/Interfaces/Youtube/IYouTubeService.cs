using YoutubeApiSynchronize.Core.Entities;

namespace YoutubeApiSynchronize.Core.Interfaces.Youtube;

public interface IYouTubeService
{
    Task<object> SyncAsync();
    Task<object> UpdateChannelStatsAsync();
    Task<Entities.Video?> GetVideoFromDbAsync(string videoId);
    Task<object?> GetVideoFromYouTubeAsync(string videoId);
    Task<object?> UpdateVideoByIdAsync(string videoId);
}
