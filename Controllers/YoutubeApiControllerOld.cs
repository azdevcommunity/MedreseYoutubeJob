// using Google.Apis.Services;
// using Google.Apis.YouTube.v3;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using YoutubeApiSyncronize.Context;
// using YoutubeApiSyncronize.Entity;
//
// namespace YoutubeApiSyncronize.Controllers;
//
// [ApiController]
// [Route("/api/[controller]")]
// public class YoutubeApiController(AppDbContext appDbContext, ILogger<YoutubeApiController> logger)
//     : ControllerBase
// {
//     private readonly YouTubeService _youtubeService = new(new BaseClientService.Initializer
//     {
//         ApiKey = DeveloperKey,
//         ApplicationName = "YouTubePlaylistFetcher"
//     });
//
//     private const string DeveloperKey = "AIzaSyC-rURatqGYAtiwWdkfkHgFUBEdxOzhUq4";
//     private const string ChannelId = "UC1B8clCxtmb-bzCDdxmVPDA";
//
//
//     [HttpPost("[action]")]
//     public async Task<IActionResult> UpdatePlaylistCount()
//     {
//         await using MedreseDbContext context = new();
//         
//         List<Playlist> playlists = await context.Playlists.ToListAsync();
//         
//         foreach (var playlist in playlists)
//         {
//             int count = await context.Videos.Where(v=>v.PlaylistId == playlist.PlaylistId).CountAsync();
//             playlist.VideoCount = count;
//         }
//         
//         await context.SaveChangesAsync();
//         return Ok(new
//         {
//             Message = "Playlist count updated"
//         });
//     }
//
//     [HttpPost("migrate")]
//     public async Task<IActionResult> Migrate()
//     {
//         await using MedreseDbContext medreseDbContext = new();
//         var list = await appDbContext.Videos.ToListAsync();
//         int addCount = 0;
//         int updateCount = 0;
//         int allVideosCount = list.Count;
//         foreach (var video in list)
//         {
//             var medreseVideo = await medreseDbContext.Videos.FirstOrDefaultAsync(v => v.VideoId == video.VideoId);
//
//             if (medreseVideo == null)
//             {
//                 logger.LogInformation($"Adding video {video.VideoId}");
//                 medreseDbContext.Videos.Add(video);
//                 addCount++;
//             }
//             else
//             {
//                 if (video.PlaylistId != medreseVideo.PlaylistId)
//                 {
//                     logger.LogInformation($"Updating video {video.VideoId}");
//                     medreseVideo.PlaylistId = video.PlaylistId;
//                     updateCount++;
//                 }
//             }
//
//             await medreseDbContext.SaveChangesAsync();
//         }
//
//         return Ok(new
//         {
//             allVideosCount,
//             addCount,
//             updateCount,
//         });
//     }
//
//     [HttpPost("sync")]
//     public async Task<IActionResult> SyncYouTubeData([FromQuery] int maxSearchResults = 50)
//     {
//         try
//         {
//             logger.LogInformation("Starting synchronization process...");
//
//             logger.LogInformation("Fetching playlists and videos...");
//             var playlists = await GetPlaylistsAsync();
//             foreach (var playlist in playlists)
//             {
//                 var videos = await GetVideosInPlaylistAsync(playlist.PlaylistId);
//                 await SavePlaylistAndVideosAsync(playlist, videos);
//             }
//
//             var searchVideos = await SearchVideosAsync(maxSearchResults);
//             await SaveVideosAsync(searchVideos);
//
//             logger.LogInformation("Synchronization completed successfully.");
//             return Ok(new { Message = "YouTube data synchronized successfully." });
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "An error occurred during synchronization.");
//             return StatusCode(500, new { Error = "An error occurred during synchronization." });
//         }
//     }
//
//     private async Task<List<Playlist>> GetPlaylistsAsync()
//     {
//         var playlists = new List<Playlist>();
//         string? nextPageToken = null;
//
//         do
//         {
//             var request = _youtubeService.Playlists.List("snippet");
//             request.ChannelId = ChannelId; // Replace with your channel ID
//             request.MaxResults = 50;
//             request.PageToken = nextPageToken;
//
//             var response = await request.ExecuteAsync();
//
//             if (response.Items.Any())
//             {
//                 playlists.AddRange(response.Items
//                     .Where(item => item.Snippet != null)
//                     .Select(item => new Playlist
//                     {
//                         PlaylistId = item.Id,
//                         Title = item.Snippet.Title,
//                         Thumbnail = item.Snippet.Thumbnails.Default__.Url + "+" +
//                                     item.Snippet.Thumbnails.Medium.Url + "+" +
//                                     item.Snippet.Thumbnails.High.Url,
//                         PublishedAt = item.Snippet.PublishedAtRaw
//                     }));
//             }
//
//
//             nextPageToken = response.NextPageToken;
//         } while (!string.IsNullOrEmpty(nextPageToken));
//
//         return playlists;
//     }
//
//     private async Task<List<Video>> GetVideosInPlaylistAsync(string playlistId)
//     {
//         var videos = new List<Video>();
//         string? nextPageToken = null;
//
//         do
//         {
//             var request = _youtubeService.PlaylistItems.List("snippet");
//             request.PlaylistId = playlistId;
//             request.MaxResults = 50;
//             request.PageToken = nextPageToken;
//
//             var response = await request.ExecuteAsync();
//             if (response.Items.Any())
//             {
//                 videos.AddRange(response.Items
//                     .Where(item => item.Snippet != null && item.Snippet.ResourceId != null)
//                     .Select(item => new Video
//                     {
//                         VideoId = item.Snippet.ResourceId.VideoId ?? string.Empty,
//                         Title = item.Snippet.Title ?? "Untitled Video",
//                         PlaylistId = playlistId,
//                         Thumbnail = string.Join("+",
//                             item.Snippet.Thumbnails?.Default__?.Url ?? string.Empty,
//                             item.Snippet.Thumbnails?.Medium?.Url ?? string.Empty,
//                             item.Snippet.Thumbnails?.High?.Url ?? string.Empty),
//                         PublishedAt = item.Snippet.PublishedAtRaw
//                     }));
//             }
//
//             nextPageToken = response.NextPageToken;
//         } while (!string.IsNullOrEmpty(nextPageToken));
//
//         return videos;
//     }
//
//     private async Task<List<Video>> SearchVideosAsync(int maxResults)
//     {
//         var videos = new List<Video>();
//         string? nextPageToken = null;
//
//         do
//         {
//             var request = _youtubeService.Search.List("snippet");
//             request.Type = "video";
//             request.MaxResults = maxResults;
//             request.PageToken = nextPageToken;
//             request.ChannelId = ChannelId;
//
//             var response = await request.ExecuteAsync();
//             if (response.Items.Any())
//             {
//                 videos.AddRange(response.Items
//                     .Where(item => item.Snippet != null)
//                     .Select(item => new Video
//                     {
//                         VideoId = item.Id.VideoId,
//                         Title = item.Snippet.Title,
//                         PlaylistId = item.Id.PlaylistId,
//                         Thumbnail = item.Snippet.Thumbnails.Default__.Url + "+" +
//                                     item.Snippet.Thumbnails.Medium.Url + "+" +
//                                     item.Snippet.Thumbnails.High.Url,
//                         PublishedAt = item.Snippet.PublishedAtRaw
//                     }));
//             }
//
//
//             nextPageToken = response.NextPageToken;
//         } while (!string.IsNullOrEmpty(nextPageToken));
//
//         return videos;
//     }
//
//     private async Task SavePlaylistAndVideosAsync(Playlist playlist, List<Video> videos)
//     {
//         var existingPlaylist = await appDbContext.Playlists
//             .FirstOrDefaultAsync(p => p.PlaylistId == playlist.PlaylistId);
//
//         if (existingPlaylist == null)
//         {
//             appDbContext.Playlists.Add(playlist);
//             await appDbContext.SaveChangesAsync();
//             existingPlaylist = playlist;
//         }
//
//         foreach (var video in videos)
//         {
//             if (appDbContext.Videos.Any(v => v.VideoId == video.VideoId)) continue;
//             video.PlaylistId = existingPlaylist.PlaylistId;
//             appDbContext.Videos.Add(video);
//             await appDbContext.SaveChangesAsync();
//         }
//     }
//
//     private async Task SaveVideosAsync(List<Video> videos)
//     {
//         foreach (var video in videos)
//         {
//             if (!appDbContext.Videos.Any(v => v.VideoId == video.VideoId))
//             {
//                 appDbContext.Videos.Add(video);
//             }
//
//             await appDbContext.SaveChangesAsync();
//         }
//     }
// }