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
    IOptions<YoutubeConfig> youtubeConfig,
    IOptions<ShortPlaylistsOptions> shortPlaylistsOptions)
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
            logger.Error(e.StackTrace, "An error occurred during synchronization.");
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
            request.ChannelId = youtubeConfig.Value.ChannelID;
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
                        Thumbnail = string.Join("+",
                            item.Snippet.Thumbnails?.Default__?.Url ?? string.Empty,
                            item.Snippet.Thumbnails?.Medium?.Url ?? string.Empty,
                            item.Snippet.Thumbnails?.High?.Url ?? string.Empty,
                            item.Snippet.Thumbnails?.Maxres?.Url ?? item.Snippet.Thumbnails?.High?.Url ?? string.Empty),
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
                if (response.Items.Any(x => x.Snippet.ResourceId.VideoId == "Eg5RrRFVrbg"))
                {
                    //normal
                    var a = response.Items.FirstOrDefault(x => x.Snippet.ResourceId.VideoId == "Eg5RrRFVrbg");
                }

                if (response.Items.Any(x => x.Snippet.ResourceId.VideoId == "rB8NWK4QK14"))
                {
                    //short
                    var a = response.Items.FirstOrDefault(x => x.Snippet.ResourceId.VideoId == "rB8NWK4QK14");
                }

                videos.AddRange(response.Items
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
                        IsPrivate = item.Snippet.Title == "Private video" ||
                                    item.Snippet.Description == "This video is private.",
                        IsShort = IsVideoShort(item.Snippet.ResourceId.VideoId, playlistId, item.Snippet.Description),
                        Description = string.IsNullOrEmpty(item.Snippet.Description) ? null : item.Snippet.Description,
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
                    if (searchResult.Id.VideoId == "Eg5RrRFVrbg")
                    {
                        //normal
                        var a = searchResult;
                    }

                    if (searchResult.Id.VideoId == "rB8NWK4QK14")
                    {
                        //short
                        var a = searchResult;
                    }

                    var newVideo = new Video
                    {
                        VideoId = searchResult.Id.VideoId,
                        Title = searchResult.Snippet.Title,
                        Thumbnail = string.Join("+",
                            searchResult.Snippet.Thumbnails?.Default__?.Url ?? string.Empty,
                            searchResult.Snippet.Thumbnails?.Medium?.Url ?? string.Empty,
                            searchResult.Snippet.Thumbnails?.High?.Url ?? string.Empty,
                            searchResult.Snippet.Thumbnails?.Maxres?.Url ??
                            searchResult.Snippet.Thumbnails?.High?.Url ?? string.Empty),
                        PublishedAt = DateTimeOffset.Parse(searchResult.Snippet.PublishedAtRaw),
                        IsPrivate = searchResult.Snippet.Title == "Private video" ||
                                    searchResult.Snippet.Description == "This video is private.",
                        IsShort = IsVideoShort(searchResult.Id.VideoId, searchResult.Id.PlaylistId,
                            searchResult.Snippet.Description),
                        Description = string.IsNullOrEmpty(searchResult.Snippet.Description)
                            ? null
                            : searchResult.Snippet.Description,
                    };

                    PlaylistVideo playlistVideo = new()
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
            else if (dbContext.Videos.Any(v => v.VideoId == video.VideoId && v.IsPrivate != video.IsPrivate))
            {
                Video foundVideo = dbContext.Videos.FirstOrDefault(v => v.VideoId == video.VideoId)!;
                foundVideo.IsPrivate = video.IsPrivate;
                foundVideo.Description = video.Description;
                foundVideo.Thumbnail = video.Thumbnail;
                foundVideo.PublishedAt = video.PublishedAt;
                foundVideo.IsShort = video.IsShort;
                foundVideo.Title = video.Title;
            }

            if (dbContext.PlaylistVideos.Any(v => v.VideoId == video.VideoId && v.PlaylistId == playlist.PlaylistId))
                continue;
            PlaylistVideo playlistVideo = new()
            {
                PlaylistId = playlist.PlaylistId,
                VideoId = video.VideoId,
            };

            dbContext.PlaylistVideos.Add(playlistVideo);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task SaveSearchVideosAsync()
    {
        try
        {
            var maxSearchResults = 50;

            (List<Video> videos, List<PlaylistVideo> playlistVideos) tuple = await SearchVideosAsync(maxSearchResults);

            var videos = tuple.videos.Where(video => !dbContext.Videos.Any(v => v.VideoId == video.VideoId));

            foreach (var video in videos)
            {
                logger.Information($"Saving video: {video.VideoId}");
                dbContext.Videos.Add(video);
                await dbContext.SaveChangesAsync();
            }

            var playlistVideos = tuple.playlistVideos.Where(playlistVideo => !dbContext.PlaylistVideos.Any(v =>
                v.VideoId == playlistVideo.VideoId && v.PlaylistId == playlistVideo.PlaylistId));

            foreach (var playlistVideo in playlistVideos)
            {
                dbContext.PlaylistVideos.Add(playlistVideo);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            logger.Error($"Error while saving videos: {e.Message}");
        }
    }

    private bool IsVideoShort(string? videoId, string playlistId, string? description)
    {
        if (videoId == "exGyJATCqVY")
        {
            Console.WriteLine("da");
        }
        if (string.IsNullOrWhiteSpace(description))
            return true;

        var shortPlaylist = shortPlaylistsOptions.Value.ShortPlaylists?
            .FirstOrDefault(p => p.PlaylistId == playlistId);

        if (shortPlaylist == null)
        {
            return false;
        }

        // Playlist tapılmayıbsa və ya exceptional list boşdursa → qısadır
        if (shortPlaylist.ExceptionalVideos == null || shortPlaylist.ExceptionalVideos.Count == 0)
            return true;

        // Playlistdə həmin video varsa → qısadır, yoxdursa → uzun
        return videoId != null && !shortPlaylist.ExceptionalVideos.Contains(videoId);
    }

    public async Task<Video?> GetVideoFromDb(string videoId)
    {
        return await dbContext.Videos.FirstOrDefaultAsync(x => x.VideoId == videoId);
    }

    public async Task<object?> GetVideoFromYoutube(string videoId)
    {
        var videoRequest = _youtubeService.Videos.List("snippet");
        videoRequest.Id = videoId;
        
        var response = await videoRequest.ExecuteAsync();
        return response;
        
        // #region MyRegion
        //
        // var request = _youtubeService.Search.List("snippet");
        // request.Type = "video";
        // request.MaxResults = 5;
        // request.PageToken = "CAUQAA";
        // request.ChannelId = youtubeConfig.Value.ChannelID;
        //
        // var response = await request.ExecuteAsync();
        //
        // foreach (var searchResult in response.Items
        //              .Where(item => item.Snippet != null))
        // {
        //     if (searchResult.Id.VideoId == "TmtEiWn3HMY")
        //     {
        //         Console.WriteLine("dasda");
        //     }
        // }
        //
        // #endregion
        //
        //
        // var request2 = _youtubeService.PlaylistItems.List("snippet");
        // request2.PlaylistId = "PLU43-RoCoSfPdyhCm8wG54l0DwYOuiTaa";
        // request2.MaxResults = 1;
        // request2.PageToken = null;
        //
        // var response2 = await request.ExecuteAsync();
        //
        //
        // return new
        // {
        //     response.NextPageToken,
        //     Search = response.Items,
        //     Elman = response2.Items
        // };
    }
}