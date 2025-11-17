using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using YoutubeApiSynchronize.Context;
using YoutubeApiSynchronize.Dto;
using YoutubeApiSynchronize.Entity;
using YoutubeApiSynchronize.Options;
using YoutubeApiSynchronize.Services;
using ILogger = Serilog.ILogger;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace YoutubeApiSynchronize.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class YoutubeController(
    YoutubeService youtubeService,
    IConfiguration configuration,
    ILogger logger,
    MedreseDbContext context,
    PubSubService pubSubService)
    : ControllerBase
{
    [HttpGet("env")]
    public IActionResult GetEnv()
    {
        try
        {
            logger.Information("GetEnv endpoint called");
            var db = configuration.GetSection("DB").Get<DatabaseSettings>()!;

            logger.Debug("Environment configuration retrieved successfully");
            return Ok(
                new
                {
                    TestValue = configuration["Test:Value"],
                    db.ConnectionString,
                }
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error in GetEnv endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("/video-db/{videoId}")]
    public async Task<IActionResult> GetVideoFromDb(string videoId)
    {
        try
        {
            logger.Information("Fetching video from database: {VideoId}", videoId);
            var result = await youtubeService.GetVideoFromDb(videoId);
            logger.Debug("Video retrieved from database: {VideoId}", videoId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error fetching video from database: {VideoId}", videoId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("/video-ytb/{videoId}")]
    public async Task<IActionResult> GetVideoFromYoutube(string videoId)
    {
        try
        {
            logger.Information("Fetching video from YouTube: {VideoId}", videoId);
            var result = await youtubeService.GetVideoFromYoutube(videoId);
            logger.Debug("Video retrieved from YouTube: {VideoId}", videoId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error fetching video from YouTube: {VideoId}", videoId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("/{videoId}")]
    public async Task<IActionResult> UpdateByVideoId(string videoId)
    {
        try
        {
            logger.Information("Updating video: {VideoId}", videoId);
            var result = await youtubeService.UpdateByVideoId(videoId);
            logger.Information("Video updated successfully: {VideoId}", videoId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating video: {VideoId}", videoId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncYouTubeData()
    {
        try
        {
            logger.Information("YouTube synchronization started via API endpoint");
            var result = await youtubeService.SyncAsync();
            logger.Information("YouTube synchronization completed successfully");
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning(ex, "Rate limit exceeded during synchronization");
            return StatusCode(429, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error during YouTube synchronization");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }


    [HttpPost("jenkins/{status}")]
    public IActionResult TurnJenkins(int status)
    {
        try
        {
            logger.Information("Jenkins control request: status={Status}", status);

            if (status != 0 && status != 1)
            {
                logger.Warning("Invalid Jenkins status parameter: {Status}", status);
                return BadRequest("Geçersiz parametre. Sadece 0 veya 1 olmalı.");
            }

            using var client = new SshClient(configuration["SERVER_HOST"], configuration["SERVER_USER_NAME"],
                configuration["SERVER_PASSWORD"]);

            logger.Debug("Connecting to SSH server");
            client.Connect();

            if (status == 1)
            {
                logger.Information("Starting Jenkins service");
                client.RunCommand("sudo systemctl start jenkins");
            }
            else
            {
                logger.Information("Stopping Jenkins service");
                client.RunCommand("sudo systemctl stop jenkins");
            }

            client.Disconnect();
            logger.Information("Jenkins service {Action} successfully", status == 1 ? "started" : "stopped");
            return Ok(new { message = $"Jenkins {(status == 1 ? "başlatıldı" : "durduruldu")}" });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error controlling Jenkins service");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("channelstat")]
    public async Task<object> GetChannelStat()
    {
        try
        {
            logger.Information("Channel statistics request received");
            var result = await youtubeService.UpdateChannelStatsAsync();
            logger.Information("Channel statistics retrieved successfully");
            return result;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving channel statistics");
            return new { error = "Internal server error" };
        }
    }


    [HttpPost("push")]
    public async Task<IActionResult> PushNotification([FromBody] object payload, [FromQuery] string challenge)
    {
        try
        {
            string jsonPayload = JsonConvert.SerializeObject(payload);

            YouTubeNotification youtubeNotificationModel = new YouTubeNotification()
            {
                NotificationData = jsonPayload,
                Title = challenge
            };

            await context.YouTubeNotifications.AddAsync(youtubeNotificationModel);
            await context.SaveChangesAsync();


            return Ok("Notification received");
        }
        catch (Exception e)
        {
            YouTubeNotification youtubeNotificationModel = new YouTubeNotification()
            {
                NotificationData = "ERROR",
                VideoId = e.GetType().Name,
                Title = $"Error: {e.Message}\nStack: {e.StackTrace}",
            };

            await context.YouTubeNotifications.AddAsync(youtubeNotificationModel);
            await context.SaveChangesAsync();

            return StatusCode(500, "Internal server error");
        }
    }


    [HttpPost("push-dlt")]
    public async Task<IActionResult> PushNotificationDlt()
    {
        try
        {
            // Request bodysini oxu
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();


            YouTubeNotification youtubeNotificationModel = new YouTubeNotification()
            {
                NotificationData = payload
            };

            await context.YouTubeNotifications.AddAsync(youtubeNotificationModel);
            await context.SaveChangesAsync();

            return Ok("Notification received");
        }
        catch (Exception e)
        {
            YouTubeNotification youtubeNotificationModel = new YouTubeNotification()
            {
                NotificationData = "ERROR",
                VideoId = e.GetType().Name,
                Title = $"Error: {e.Message}\nStack: {e.StackTrace}",
            };

            await context.YouTubeNotifications.AddAsync(youtubeNotificationModel);
            await context.SaveChangesAsync();

            return StatusCode(500, "Internal server error");
        }
    }

    private (string videoId, string title, DateTime? publishedAt) ParseYouTubeNotification(string payload)
    {
        try
        {
            // XML parsing
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(payload);

            var nsManager = new System.Xml.XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("yt", "http://www.youtube.com/xml/schemas/2015");
            nsManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");

            var videoId = doc.SelectSingleNode("//yt:videoId", nsManager)?.InnerText;
            var title = doc.SelectSingleNode("//atom:title", nsManager)?.InnerText;
            var publishedAtStr = doc.SelectSingleNode("//atom:published", nsManager)?.InnerText;

            DateTime? publishedAt = null;
            if (DateTime.TryParse(publishedAtStr, out var parsedDate))
            {
                publishedAt = parsedDate;
            }

            YouTubeNotification youtubeNotificationModel = new YouTubeNotification()
            {
                NotificationData = nsManager.ToString(),
                VideoId = videoId,
                Title = title,
            };

            context.YouTubeNotifications.Add(youtubeNotificationModel);
            context.SaveChanges();

            return (videoId, title, publishedAt);
        }
        catch (Exception ex)
        {
            YouTubeNotification youtubeNotificationModel = new YouTubeNotification()
            {
                NotificationData = ex.ToString(),
            };

            context.YouTubeNotifications.Add(youtubeNotificationModel);
            context.SaveChanges();
            // Parse error olarsa, əsas məlumatları saxla
            return (null, "Parse Error", DateTime.UtcNow);
        }
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] string payload, [FromQuery] string challenge)
    {
        await pubSubService.SubscribeToTopic(payload, challenge);
        return Ok("Sucess");
    }

    [HttpPost("UnsubscribeAndUpdateCallbackUrl")]
    public async Task<IActionResult> UnsubscribeAndUpdateCallbackUrl()
    {
        await pubSubService.UnsubscribeAndUpdateCallbackUrl();

        return Ok("Subscription updated successfully.");
    }
}