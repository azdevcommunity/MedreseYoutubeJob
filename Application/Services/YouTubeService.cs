using YoutubeApiSynchronize.Core.Entities;
using YoutubeApiSynchronize.Core.Interfaces;
using Microsoft.Extensions.Options;
using YoutubeApiSynchronize.Options;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.Application.Services;

public class YouTubeService : IYouTubeService
{
    private readonly IYouTubeRepository _repository;
    private readonly IYouTubeApiClient _apiClient;
    private readonly ILogger _logger;
    private readonly YoutubeConfig _youtubeConfig;
    private readonly ShortPlaylistsOptions _shortPlaylistsOptions;

    public YouTubeService(
        IYouTubeRepository repository,
        IYouTubeApiClient apiClient,
        ILogger logger,
        IOptions<YoutubeConfig> youtubeConfig,
        IOptions<ShortPlaylistsOptions> shortPlaylistsOptions)
    {
        _repository = repository;
        _apiClient = apiClient;
        _logger = logger;
        _youtubeConfig = youtubeConfig.Value;
        _shortPlaylistsOptions = shortPlaylistsOptions.Value;
    }

    public async Task<object> SyncAsync()
    {
        try
        {
            _logger.Information("Starting synchronization process...");

            await ProcessPlaylistsAndVideosAsync();
            await SaveSearchVideosAsync();
            await _repository.UpdatePlaylistVideoCountsAsync();

            _logger.Information("Synchronization completed successfully.");
            return new { Message = "YouTube data synchronized successfully." };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred during synchronization.");
            return new { Error = "An error occurred during synchronization." };
        }
    }

    public async Task<object> UpdateChannelStatsAsync()
    {
        try
        {
            _logger.Information("Updating channel statistics for channel: {ChannelId}", _youtubeConfig.ChannelID);

            var channelStat = await _repository.GetChannelStatAsync() ?? new ChannelStat();
            var response = await _apiClient.GetChannelStatisticsAsync(_youtubeConfig.ChannelID);
            var stats = response.Items[0].Statistics;

            channelStat.SubscriberCount = stats.SubscriberCount;
            channelStat.ViewCount = stats.ViewCount;
            channelStat.HiddenSubscriberCount = stats.HiddenSubscriberCount;
            channelStat.VideoCount = stats.VideoCount;

            await _repository.UpdateChannelStatAsync(channelStat);
            await _repository.SaveChangesAsync();

            _logger.Information("Channel statistics updated successfully. Subscribers: {Subscribers}, Views: {Views}, Videos: {Videos}",
                stats.SubscriberCount, stats.ViewCount, stats.VideoCount);

            return channelStat;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating channel statistics");
            throw;
        }
    }

    public async Task<Video?> GetVideoFromDbAsync(string videoId)
    {
        return await _repository.GetVideoByIdAsync(videoId);
    }

    public async Task<object?> GetVideoFromYouTubeAsync(string videoId)
    {
        return await _apiClient.GetVideoDetailsAsync(videoId);
    }

    public async Task<object?> UpdateVideoByIdAsync(string videoId)
    {
        var response = await _apiClient.GetVideoDetailsAsync(videoId);
        var ytbVideo = response.Items.FirstOrDefault(x => x.Id == videoId);

        if (ytbVideo == null)
        {
            return new { Message = "Video not found on youtube" };
        }

        var video = await _repository.GetVideoByIdAsync(videoId);
        if (video == null)
        {
            return new { Message = "Video not found on db" };
        }

        video.Description = ytbVideo.Snippet.Description;
        video.Thumbnail = string.Join("+",
            ytbVideo.Snippet.Thumbnails?.Default__?.Url ?? string.Empty,
            ytbVideo.Snippet.Thumbnails?.Medium?.Url ?? string.Empty,
            ytbVideo.Snippet.Thumbnails?.High?.Url ?? string.Empty,
            ytbVideo.Snippet.Thumbnails?.Maxres?.Url ?? ytbVideo.Snippet.Thumbnails?.High?.Url ?? string.Empty);
        video.Title = ytbVideo.Snippet.Title;
        video.PublishedAt = DateTimeOffset.Parse(ytbVideo.Snippet.PublishedAtRaw);
        video.IsPrivate = ytbVideo.Snippet.Title == "Private video" || ytbVideo.Snippet.Description == "This video is private.";

        await _repository.UpdateVideoAsync(video);
        await _repository.SaveChangesAsync();

        return video;
    }

    private async Task ProcessPlaylistsAndVideosAsync()
    {
        _logger.Information("Fetching playlists and videos...");
        try
        {
            var playlists = await GetPlaylistsAsync();
            foreach (var playlist in playlists)
            {
                var videos = await GetVideosInPlaylistAsync(playlist.PlaylistId);
                await SavePlaylistAndVideosAsync(playlist, videos);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occurred during ProcessPlaylistsAndVideos.");
        }
    }

    private async Task<List<Playlist>> GetPlaylistsAsync()
    {
        var playlists = new List<Playlist>();
        string? nextPageToken = null;
        int pageCount = 0;

        try
        {
            _logger.Information("Fetching playlists for channel: {ChannelId}", _youtubeConfig.ChannelID);

            do
            {
                var response = await _apiClient.GetPlaylistsAsync(_youtubeConfig.ChannelID, nextPageToken);

                if (response.Items.Any())
                {
                    pageCount++;
                    var playlistsInPage = response.Items
                        .Where(item => item.Snippet != null)
                        .Select(item => new Playlist
                        {
                            PlaylistId = item.Id,
                            Title = item.Snippet.Title,
                            Thumbnail = string.Join("+",
                                item.Snippet.Thumbnails?.Default__?.Url ?? string.Empty,
                                item.Snippet.Thumbnails?.Medium?.Url ?? string.Empty,
                                item.Snippet.Thumbnails?.High?.Url ?? string.Empty,
                                item.Snippet.Thumbnails?.Maxres?.Url ?? item.Snippet.Thumbnails?.High?.Url ?? string.Empty),
                            PublishedAt = DateTimeOffset.Parse(item.Snippet.PublishedAtRaw)
                        }).ToList();

                    playlists.AddRange(playlistsInPage);
                    _logger.Debug("Fetched {Count} playlists in page {PageNumber}", playlistsInPage.Count, pageCount);
                }

                nextPageToken = response.NextPageToken;
            } while (!string.IsNullOrEmpty(nextPageToken));

            _logger.Information("Successfully fetched {TotalCount} playlists from {PageCount} pages", playlists.Count, pageCount);
            return playlists;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching playlists");
            throw;
        }
    }

    private async Task<List<Video>> GetVideosInPlaylistAsync(string playlistId)
    {
        var videos = new List<Video>();
        string? nextPageToken = null;
        int pageCount = 0;

        try
        {
            _logger.Information("Fetching videos for playlist: {PlaylistId}", playlistId);

            do
            {
                var response = await _apiClient.GetPlaylistItemsAsync(playlistId, nextPageToken);
                
                if (response.Items.Any())
                {
                    pageCount++;
                    var videosInPage = response.Items
                        .Where(item => item.Snippet != null && item.Snippet.ResourceId != null)
                        .Select(item => new Video
                        {
                            VideoId = item.Snippet.ResourceId.VideoId ?? string.Empty,
                            Title = item.Snippet.Title ?? "Untitled Video",
                            Thumbnail = string.Join("+",
                                item.Snippet.Thumbnails?.Default__?.Url ?? string.Empty,
                                item.Snippet.Thumbnails?.Medium?.Url ?? string.Empty,
                                item.Snippet.Thumbnails?.High?.Url ?? string.Empty,
                                item.Snippet.Thumbnails?.Maxres?.Url ?? item.Snippet.Thumbnails?.High?.Url ?? string.Empty),
                            PublishedAt = DateTimeOffset.Parse(item.Snippet.PublishedAtRaw),
                            IsPrivate = item.Snippet.Title == "Private video" || item.Snippet.Description == "This video is private.",
                            IsShort = IsVideoShort(item.Snippet.ResourceId.VideoId, playlistId, item.Snippet.Description),
                            Description = string.IsNullOrEmpty(item.Snippet.Description) ? null : item.Snippet.Description,
                        }).ToList();

                    videos.AddRange(videosInPage);
                    _logger.Debug("Fetched {Count} videos in page {PageNumber} for playlist {PlaylistId}",
                        videosInPage.Count, pageCount, playlistId);
                }

                nextPageToken = response.NextPageToken;
            } while (!string.IsNullOrEmpty(nextPageToken));

            _logger.Information("Successfully fetched {TotalCount} videos from {PageCount} pages for playlist {PlaylistId}",
                videos.Count, pageCount, playlistId);
            return videos;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching videos for playlist {PlaylistId}", playlistId);
            throw;
        }
    }

    private async Task SavePlaylistAndVideosAsync(Playlist playlist, List<Video> videos)
    {
        var existingPlaylist = await _repository.GetPlaylistByIdAsync(playlist.PlaylistId);

        if (existingPlaylist == null)
        {
            await _repository.AddPlaylistAsync(playlist);
            await _repository.SaveChangesAsync();
        }
        else if (existingPlaylist.Thumbnail != playlist.Thumbnail)
        {
            existingPlaylist.Thumbnail = playlist.Thumbnail;
            await _repository.UpdatePlaylistAsync(existingPlaylist);
            await _repository.SaveChangesAsync();
        }

        foreach (var video in videos)
        {
            var existingVideo = await _repository.GetVideoByIdAsync(video.VideoId);
            
            if (existingVideo == null)
            {
                await _repository.AddVideoAsync(video);
                await _repository.SaveChangesAsync();
            }
            else if (existingVideo.IsPrivate != video.IsPrivate)
            {
                existingVideo.IsPrivate = video.IsPrivate;
                existingVideo.Description = video.Description;
                existingVideo.Thumbnail = video.Thumbnail;
                existingVideo.PublishedAt = video.PublishedAt;
                existingVideo.IsShort = video.IsShort;
                existingVideo.Title = video.Title;
                await _repository.UpdateVideoAsync(existingVideo);
            }

            if (await _repository.PlaylistVideoExistsAsync(video.VideoId, playlist.PlaylistId))
                continue;

            var playlistVideo = new PlaylistVideo
            {
                PlaylistId = playlist.PlaylistId,
                VideoId = video.VideoId,
            };

            await _repository.AddPlaylistVideoAsync(playlistVideo);
            await _repository.SaveChangesAsync();
        }
    }

    private async Task SaveSearchVideosAsync()
    {
        try
        {
            const int maxSearchResults = 50;
            var (videos, playlistVideos) = await SearchVideosAsync(maxSearchResults);

            foreach (var video in videos)
            {
                if (!await _repository.VideoExistsAsync(video.VideoId))
                {
                    _logger.Information("Saving video: {VideoId}", video.VideoId);
                    await _repository.AddVideoAsync(video);
                    await _repository.SaveChangesAsync();
                }
            }

            foreach (var playlistVideo in playlistVideos)
            {
                if (!await _repository.PlaylistVideoExistsAsync(playlistVideo.VideoId, playlistVideo.PlaylistId))
                {
                    await _repository.AddPlaylistVideoAsync(playlistVideo);
                    await _repository.SaveChangesAsync();
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error while saving videos");
        }
    }

    private async Task<(List<Video>, List<PlaylistVideo>)> SearchVideosAsync(int maxResults)
    {
        var videos = new List<Video>();
        var playlistVideos = new List<PlaylistVideo>();
        string? nextPageToken = null;

        do
        {
            var response = await _apiClient.SearchVideosAsync(_youtubeConfig.ChannelID, maxResults, nextPageToken);
            
            if (response.Items.Any())
            {
                foreach (var searchResult in response.Items.Where(item => item.Snippet != null))
                {
                    var newVideo = new Video
                    {
                        VideoId = searchResult.Id.VideoId,
                        Title = searchResult.Snippet.Title,
                        Thumbnail = string.Join("+",
                            searchResult.Snippet.Thumbnails?.Default__?.Url ?? string.Empty,
                            searchResult.Snippet.Thumbnails?.Medium?.Url ?? string.Empty,
                            searchResult.Snippet.Thumbnails?.High?.Url ?? string.Empty,
                            searchResult.Snippet.Thumbnails?.Maxres?.Url ?? searchResult.Snippet.Thumbnails?.High?.Url ?? string.Empty),
                        PublishedAt = DateTimeOffset.Parse(searchResult.Snippet.PublishedAtRaw),
                        IsPrivate = searchResult.Snippet.Title == "Private video" || searchResult.Snippet.Description == "This video is private.",
                        IsShort = IsVideoShort(searchResult.Id.VideoId, searchResult.Id.PlaylistId, searchResult.Snippet.Description),
                        Description = string.IsNullOrEmpty(searchResult.Snippet.Description) ? null : searchResult.Snippet.Description,
                    };

                    var playlistVideo = new PlaylistVideo
                    {
                        VideoId = newVideo.VideoId ?? string.Empty,
                        PlaylistId = searchResult.Id.PlaylistId ?? string.Empty,
                    };

                    videos.Add(newVideo);
                    playlistVideos.Add(playlistVideo);
                }
            }

            nextPageToken = response.NextPageToken;
        } while (!string.IsNullOrEmpty(nextPageToken));

        return (videos, playlistVideos);
    }

    private bool IsVideoShort(string? videoId, string playlistId, string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return true;

        var shortPlaylist = _shortPlaylistsOptions.ShortPlaylists?
            .FirstOrDefault(p => p.PlaylistId == playlistId);

        if (shortPlaylist == null)
            return false;

        if (shortPlaylist.ExceptionalVideos == null || shortPlaylist.ExceptionalVideos.Count == 0)
            return true;

        return videoId != null && !shortPlaylist.ExceptionalVideos.Contains(videoId);
    }
}
