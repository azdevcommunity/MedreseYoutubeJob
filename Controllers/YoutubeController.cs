using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
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

    public YoutubeController(YoutubeService youtubeService, IConfiguration configuration,
        ILogger logger)
    {
        _youtubeService = youtubeService;
        _configuration = configuration;
        _logger = logger;
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


    [Route("api/[controller]")]
    [ApiController]
    public class YouTubeController : ControllerBase
    {
        [HttpPost("push")]
        public async Task<IActionResult> PushNotification([FromBody] string payload, [FromQuery] string challenge)
        {
            // İlk bildirimde gelen hub.challenge parametresini doğrulama
            if (!string.IsNullOrEmpty(challenge))
            {
                return Ok(challenge); // Challenge'ı geri göndererek doğrulama sağlanır
            }

            // Gelen bildirim verisini işleme
            var json = JObject.Parse(payload);
            var videoId = json["video_id"]?.ToString();
            var title = json["title"]?.ToString();
            var publishedAt = json["published"]?.ToString();

            // Burada veriyi işleyebilirsiniz (veritabanına kaydedebilir veya bildirim gönderebilirsiniz)
            Console.WriteLine($"Video ID: {videoId}, Title: {title}, Published: {publishedAt}");

            return Ok("Notification received");
        }
    }

 
}