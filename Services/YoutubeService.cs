using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YoutubeApiSynchronize.Context;
using YoutubeApiSynchronize.Entity;
using YoutubeApiSynchronize.Options;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.Services;

public class YoutubeService(
    MedreseDbContext dbContext,
    ILogger logger,
    IOptions<YoutubeConfig> youtubeConfig)
{
    private readonly YouTubeService _youtubeService = new(new BaseClientService.Initializer
    {
        ApiKey = youtubeConfig.Value.DeveloperKey,
        ApplicationName = "YouTubePlaylistFetcher"
    });


    
    
    public async Task<object> SyncAsync()
    {
        try
        {
            logger.Information("Starting synchronization process... {}", new DateTime());

            await ProcessPlaylistsAndVideos();

            await SaveSearchVideosAsync();

            await UpdatePlaylistCountAsync();

            logger.Information("Synchronization completed successfully.");
            return new { Message = "YouTube data synchronized successfully." };
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred during synchronization.");
            return new { Error = "An error occurred during synchronization." };
        }
    }

    private async Task ProcessPlaylistsAndVideos()
    {
        logger.Information("Fetching playlists and videos...");
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
            logger.Error(e, "An error occurred during ProcessPlaylistsAndVideos.");
        }
    }

    private async Task UpdatePlaylistCountAsync()
    {
        try
        {
            var playlists = await dbContext.Playlists.ToListAsync();

            foreach (var playlist in playlists)
            {
                var count = await dbContext.PlaylistVideos.CountAsync(x => x.PlaylistId == playlist.PlaylistId);
                playlist.VideoCount = count;
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.Error(e, "An error occurred during UpdatePlaylistCountAsync.");
        }
    }

    private async Task<List<Playlist>> GetPlaylistsAsync()
    {
        var playlists = new List<Playlist>();
        string? nextPageToken = null;

        do
        {
            var request = _youtubeService.Playlists.List("snippet");
            request.ChannelId = youtubeConfig.Value.ChannelID; // Replace with your channel ID
            request.MaxResults = 50;
            request.PageToken = nextPageToken;

            var response = await request.ExecuteAsync();

            if (response.Items.Any())
            {
                playlists.AddRange(response.Items
                    .Where(item => item.Snippet != null)
                    .Select(item => new Playlist
                    {
                        PlaylistId = item.Id,
                        Title = item.Snippet.Title,
                        Thumbnail = item.Snippet.Thumbnails.Default__.Url + "+" +
                                    item.Snippet.Thumbnails.Medium.Url + "+" +
                                    item.Snippet.Thumbnails.High.Url,
                        PublishedAt = DateTimeOffset.Parse(item.Snippet.PublishedAtRaw)
                    }));
            }


            nextPageToken = response.NextPageToken;
        } while (!string.IsNullOrEmpty(nextPageToken));

        return playlists;
    }

    private async Task<List<Video>> GetVideosInPlaylistAsync(string playlistId)
    {
        var videos = new List<Video>();
        string? nextPageToken = null;

        do
        {
            var request = _youtubeService.PlaylistItems.List("snippet");
            request.PlaylistId = playlistId;
            request.MaxResults = 50;
            request.PageToken = nextPageToken;

            var response = await request.ExecuteAsync();
            if (response.Items.Any())
            {
                videos.AddRange(response.Items
                    .Where(item => item.Snippet != null && item.Snippet.ResourceId != null)
                    .Select(item => new Video
                    {
                        VideoId = item.Snippet.ResourceId.VideoId ?? string.Empty,
                        Title = item.Snippet.Title ?? "Untitled Video",
                        Thumbnail = string.Join("+",
                            item.Snippet.Thumbnails?.Default__?.Url ?? string.Empty,
                            item.Snippet.Thumbnails?.Medium?.Url ?? string.Empty,
                            item.Snippet.Thumbnails?.High?.Url ?? string.Empty),
                        PublishedAt = DateTimeOffset.Parse(item.Snippet.PublishedAtRaw)
                    }));
            }

            nextPageToken = response.NextPageToken;
        } while (!string.IsNullOrEmpty(nextPageToken));

        return videos;
    }

    private async Task<(List<Video>, List<PlaylistVideo>)> SearchVideosAsync(int maxResults)
    {
        var videos = new List<Video>();
        var playlistVideos = new List<PlaylistVideo>();
        string? nextPageToken = null;

        do
        {
            var request = _youtubeService.Search.List("snippet");
            request.Type = "video";
            request.MaxResults = maxResults;
            request.PageToken = nextPageToken;
            request.ChannelId = youtubeConfig.Value.ChannelID;

            var response = await request.ExecuteAsync();
            if (response.Items.Any())
            {
                foreach (var searchResult in response.Items
                             .Where(item => item.Snippet != null))
                {
                    var newVideo = new Video
                    {
                        VideoId = searchResult.Id.VideoId,
                        Title = searchResult.Snippet.Title,
                        Thumbnail = searchResult.Snippet.Thumbnails.Default__.Url + "+" +
                                    searchResult.Snippet.Thumbnails.Medium.Url + "+" +
                                    searchResult.Snippet.Thumbnails.High.Url,
                        PublishedAt = DateTimeOffset.Parse(searchResult.Snippet.PublishedAtRaw)
                    };

                    PlaylistVideo playlistVideo = new()
                    {
                        PlaylistId = newVideo.VideoId ?? string.Empty,
                        VideoId = searchResult.Id.PlaylistId ?? string.Empty,
                    };

                    videos.Add(newVideo);
                    playlistVideos.Add(playlistVideo);
                }
            }


            nextPageToken = response.NextPageToken;
        } while (!string.IsNullOrEmpty(nextPageToken));

        return (videos, playlistVideos);
    }

    private async Task SavePlaylistAndVideosAsync(Playlist playlist, List<Video> videos)
    {
        var existingPlaylist = await dbContext.Playlists
            .FirstOrDefaultAsync(p => p.PlaylistId == playlist.PlaylistId);

        if (existingPlaylist == null)
        {
            dbContext.Playlists.Add(playlist);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            if (existingPlaylist.Thumbnail != playlist.Thumbnail)
            {
                existingPlaylist.Thumbnail = playlist.Thumbnail;
                await dbContext.SaveChangesAsync();
            }
        }

        foreach (var video in videos)
        {
            if (!dbContext.Videos.Any(v => v.VideoId == video.VideoId))
            {
                dbContext.Videos.Add(video);
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.PlaylistVideos.Any(v => v.VideoId == video.VideoId && v.PlaylistId == playlist.PlaylistId))
            {
                PlaylistVideo playlistVideo = new()
                {
                    PlaylistId = playlist.PlaylistId,
                    VideoId = video.VideoId,
                };

                dbContext.PlaylistVideos.Add(playlistVideo);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task SaveSearchVideosAsync()
    {
        try
        {
            int maxSearchResults = 50;

            (List<Video> videos, List<PlaylistVideo> playlistVideos) tuple = await SearchVideosAsync(maxSearchResults);

            foreach (var video in tuple.videos)
            {
                if (!dbContext.Videos.Any(v => v.VideoId == video.VideoId))
                {
                    logger.Information($"Saving video: {video.VideoId}");
                    dbContext.Videos.Add(video);
                    await dbContext.SaveChangesAsync();
                }
            }

            foreach (var playlistVideo in tuple.playlistVideos)
            {
                if (!dbContext.PlaylistVideos.Any(v =>
                        v.VideoId == playlistVideo.VideoId && v.PlaylistId == playlistVideo.PlaylistId))
                {
                    dbContext.PlaylistVideos.Add(playlistVideo);
                    await dbContext.SaveChangesAsync();
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"Error while saving videos: {e.Message}");
        }
    }
}