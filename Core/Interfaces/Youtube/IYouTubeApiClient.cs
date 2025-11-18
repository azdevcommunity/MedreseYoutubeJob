using Google.Apis.YouTube.v3.Data;

namespace YoutubeApiSynchronize.Core.Interfaces.Youtube;

public interface IYouTubeApiClient
{
    Task<PlaylistListResponse> GetPlaylistsAsync(string channelId, string? pageToken = null);
    Task<PlaylistItemListResponse> GetPlaylistItemsAsync(string playlistId, string? pageToken = null);
    Task<SearchListResponse> SearchVideosAsync(string channelId, int maxResults, string? pageToken = null);
    Task<VideoListResponse> GetVideoDetailsAsync(string videoId);
    Task<ChannelListResponse> GetChannelStatisticsAsync(string channelId);
}
