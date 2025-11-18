using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Options;
using YoutubeApiSynchronize.Core.Interfaces;
using YoutubeApiSynchronize.Options;

namespace YoutubeApiSynchronize.Infrastructure.ExternalServices;

public class YouTubeApiClient : IYouTubeApiClient
{
    private readonly YouTubeService _youtubeService;

    public YouTubeApiClient(IOptions<YoutubeConfig> youtubeConfig)
    {
        _youtubeService = new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = youtubeConfig.Value.DeveloperKey,
            ApplicationName = "YouTubePlaylistFetcher"
        });
    }

    public async Task<PlaylistListResponse> GetPlaylistsAsync(string channelId, string? pageToken = null)
    {
        var request = _youtubeService.Playlists.List("snippet");
        request.ChannelId = channelId;
        request.MaxResults = 50;
        request.PageToken = pageToken;

        return await request.ExecuteAsync();
    }

    public async Task<PlaylistItemListResponse> GetPlaylistItemsAsync(string playlistId, string? pageToken = null)
    {
        var request = _youtubeService.PlaylistItems.List("snippet");
        request.PlaylistId = playlistId;
        request.MaxResults = 50;
        request.PageToken = pageToken;

        return await request.ExecuteAsync();
    }

    public async Task<SearchListResponse> SearchVideosAsync(string channelId, int maxResults, string? pageToken = null)
    {
        var request = _youtubeService.Search.List("snippet");
        request.Type = "video";
        request.MaxResults = maxResults;
        request.PageToken = pageToken;
        request.ChannelId = channelId;

        return await request.ExecuteAsync();
    }

    public async Task<VideoListResponse> GetVideoDetailsAsync(string videoId)
    {
        var request = _youtubeService.Videos.List("snippet");
        request.Id = videoId;

        return await request.ExecuteAsync();
    }

    public async Task<ChannelListResponse> GetChannelStatisticsAsync(string channelId)
    {
        var request = _youtubeService.Channels.List("statistics");
        request.Id = channelId;

        return await request.ExecuteAsync();
    }
}
