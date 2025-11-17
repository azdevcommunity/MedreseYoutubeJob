using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using YoutubeApiSynchronize.Context;
using YoutubeApiSynchronize.Dto;
using YoutubeApiSynchronize.Options;
using YoutubeApiSynchronize.Services;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class YoutubeController
    : ControllerBase
{
    private readonly YoutubeService _youtubeService;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly MedreseDbContext _context;

    public YoutubeController(YoutubeService youtubeService, IConfiguration configuration,
        ILogger logger, MedreseDbContext context)
    {
        _youtubeService = youtubeService;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    [HttpGet("env")]
    public IActionResult GetEnv()
    {
        try
        {
            _logger.Information("GetEnv endpoint called");
            var db = _configuration.GetSection("DB").Get<DatabaseSettings>()!;

            _logger.Debug("Environment configuration retrieved successfully");
            return Ok(
                new
                {
                    TestValue = _configuration["Test:Value"],
                    db.ConnectionString,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetEnv endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("/video-db/{videoId}")]
    public async Task<IActionResult> GetVideoFromDb(string videoId)
    {
        try
        {
            _logger.Information("Fetching video from database: {VideoId}", videoId);
            var result = await _youtubeService.GetVideoFromDb(videoId);
            _logger.Debug("Video retrieved from database: {VideoId}", videoId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching video from database: {VideoId}", videoId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("/video-ytb/{videoId}")]
    public async Task<IActionResult> GetVideoFromYoutube(string videoId)
    {
        try
        {
            _logger.Information("Fetching video from YouTube: {VideoId}", videoId);
            var result = await _youtubeService.GetVideoFromYoutube(videoId);
            _logger.Debug("Video retrieved from YouTube: {VideoId}", videoId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching video from YouTube: {VideoId}", videoId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("/{videoId}")]
    public async Task<IActionResult> UpdateByVideoId(string videoId)
    {
        try
        {
            _logger.Information("Updating video: {VideoId}", videoId);
            var result = await _youtubeService.UpdateByVideoId(videoId);
            _logger.Information("Video updated successfully: {VideoId}", videoId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating video: {VideoId}", videoId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncYouTubeData()
    {
        try
        {
            _logger.Information("YouTube synchronization started via API endpoint");
            var result = await _youtubeService.SyncAsync();
            _logger.Information("YouTube synchronization completed successfully");
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warning(ex, "Rate limit exceeded during synchronization");
            return StatusCode(429, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during YouTube synchronization");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }


    [HttpPost("jenkins/{status}")]
    public IActionResult TurnJenkins(int status)
    {
        try
        {
            _logger.Information("Jenkins control request: status={Status}", status);

            if (status != 0 && status != 1)
            {
                _logger.Warning("Invalid Jenkins status parameter: {Status}", status);
                return BadRequest("Geçersiz parametre. Sadece 0 veya 1 olmalı.");
            }

            using var client = new SshClient(_configuration["SERVER_HOST"], _configuration["SERVER_USER_NAME"],
                _configuration["SERVER_PASSWORD"]);

            _logger.Debug("Connecting to SSH server");
            client.Connect();

            if (status == 1)
            {
                _logger.Information("Starting Jenkins service");
                client.RunCommand("sudo systemctl start jenkins");
            }
            else
            {
                _logger.Information("Stopping Jenkins service");
                client.RunCommand("sudo systemctl stop jenkins");
            }

            client.Disconnect();
            _logger.Information("Jenkins service {Action} successfully", status == 1 ? "started" : "stopped");
            return Ok(new { message = $"Jenkins {(status == 1 ? "başlatıldı" : "durduruldu")}" });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error controlling Jenkins service");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("channelstat")]
    public async Task<object> GetChannelStat()
    {
        try
        {
            _logger.Information("Channel statistics request received");
            var result = await _youtubeService.UpdateChannelStatsAsync();
            _logger.Information("Channel statistics retrieved successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving channel statistics");
            return new { error = "Internal server error" };
        }
    }


    [HttpPost("push")]
    public async Task<IActionResult> PushNotification([FromBody] string payload, [FromQuery] string challenge)
    {
        if (!string.IsNullOrEmpty(challenge))
        {
            return Ok(challenge);
        }

        var json = JObject.Parse(payload);
        var videoId = json["video_id"]?.ToString();
        var title = json["title"]?.ToString();
        var publishedAt = json["published"]?.ToString();

        var query =
            "INSERT INTO youtube_notifications (video_id, title, published_at, notification_data, created_at) " +
            "VALUES (@p0, @p1, @p2, @p3, CURRENT_TIMESTAMP)";

        await _context.Database.ExecuteSqlRawAsync(query, videoId, title, publishedAt, payload);

        Console.WriteLine($"Video ID: {videoId}, Title: {title}, Published: {publishedAt}");

        return Ok("Notification received");
    }
    
    
    [HttpPost("push-dlt")]
    public async Task<IActionResult> PushNotificationDlt([FromBody] string payload, [FromQuery] string challenge)
    {
        if (!string.IsNullOrEmpty(challenge))
        {
            return Ok(challenge);
        }

        // XML verisini işlemek için XmlSerializer kullan
        var serializer = new XmlSerializer(typeof(YoutubeNotificationModel));
        YoutubeNotificationModel notificationModel;

        using (var reader = new StringReader(payload))
        {
            notificationModel = (YoutubeNotificationModel)serializer.Deserialize(reader);
        }

        // Gelen veriyi konsola yazdır
        Console.WriteLine($"Video ID: {notificationModel.VideoId}, Title: {notificationModel.Title}, Published: {notificationModel.PublishedAt}");

        var query = 
            "INSERT INTO youtube_notifications (video_id, title, published_at, notification_data, created_at) " +
            "VALUES (@p0, @p1, @p2, @p3, CURRENT_TIMESTAMP)";

        // Veriyi veritabanına kaydet
        await _context.Database.ExecuteSqlRawAsync(query, notificationModel.VideoId, notificationModel.Title, notificationModel.PublishedAt, payload);

        return Ok("Notification received");
    }



    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] string payload, [FromQuery] string challenge)
    {
        string channelId = "UCN22jHS7MPBp38ZWZemt7i";
        string callbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/Youtube/push";

        string hubUrl = "https://pubsubhubbub.appspot.com/subscribe";
        string topicUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={channelId}";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hub.callback", callbackUrl),
            new KeyValuePair<string, string>("hub.mode", "subscribe"),
            new KeyValuePair<string, string>("hub.topic", topicUrl),
            new KeyValuePair<string, string>("hub.verify", "sync")
        });

        using (var client = new HttpClient())
        {
            using var response = await client.PostAsync(hubUrl, content);

            _logger.Information(response.IsSuccessStatusCode
                ? "Successfully subscribed to the channel."
                : $"Failed to subscribe. Status code: {response.StatusCode}");
        }

        return Ok("Sucess");
    }

    [HttpPost("UnsubscribeAndUpdateCallbackUrl")]
    public async Task<IActionResult> UnsubscribeAndUpdateCallbackUrl()
    {
        string channelId = "UCN22jHS7MPBp38ZWZemt7i";
        string newCallbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/Youtube/push";
        string oldCallbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/youtube-pubsub/push";
        string hubUrl = "https://pubsubhubbub.appspot.com/subscribe";
        string topicUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={channelId}";

        // Unsubscribe işlemi (Eski callback URL ile)
        var unsubscribeContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hub.callback", oldCallbackUrl),
            new KeyValuePair<string, string>("hub.mode", "unsubscribe"),
            new KeyValuePair<string, string>("hub.topic", topicUrl),
        });

        using (var client = new HttpClient())
        {
            var unsubscribeResponse = await client.PostAsync(hubUrl, unsubscribeContent);
            if (unsubscribeResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Unsubscribed successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to unsubscribe. Status code: {unsubscribeResponse.StatusCode}");
                return BadRequest("Unsubscribe failed.");
            }
        }

        // Subscribe işlemi (Yeni callback URL ile)
        var subscribeContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hub.callback", newCallbackUrl),
            new KeyValuePair<string, string>("hub.mode", "subscribe"),
            new KeyValuePair<string, string>("hub.topic", topicUrl),
            new KeyValuePair<string, string>("hub.verify", "sync")
        });

        using (var client = new HttpClient())
        {
            var subscribeResponse = await client.PostAsync(hubUrl, subscribeContent);
            if (subscribeResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Successfully subscribed with new callback URL.");
            }
            else
            {
                Console.WriteLine($"Failed to subscribe. Status code: {subscribeResponse.StatusCode}");
                return BadRequest("Subscribe failed.");
            }
        }

        return Ok("Subscription updated successfully.");
    }
}


public class YoutubeNotificationModel
{
    [XmlElement("video_id")]
    public string VideoId { get; set; }

    [XmlElement("title")]
    public string Title { get; set; }

    [XmlElement("published")]
    public string PublishedAt { get; set; }
}