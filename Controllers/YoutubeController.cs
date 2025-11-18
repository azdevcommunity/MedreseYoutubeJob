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
    public async Task<IActionResult> PushNotification()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        YouTubeNotification youtubeNotificationModel = new YouTubeNotification()
        {
            NotificationData = payload,
        };

        await context.YouTubeNotifications.AddAsync(youtubeNotificationModel);
        await context.SaveChangesAsync();


        return Ok("Notification received");
    }

    [HttpGet("push")]
    public async Task<IActionResult> Push(
        [FromQuery(Name = "hub.mode")] string hubMode,
        [FromQuery(Name = "hub.topic")] string hubTopic,
        [FromQuery(Name = "hub.challenge")] string hubChallenge,
        [FromQuery(Name = "hub.lease_seconds")]
        int hubLeaseSeconds)
    {
        if (hubMode == "subscribe")
        {
            return Ok(hubChallenge);
        }
     
        await pubSubService.PushAsync(hubMode, hubTopic, hubChallenge, hubLeaseSeconds,  Request.Host.Value);

        return Ok("Notification received");
    }


    [HttpPost("push-dlt")]
    public async Task<IActionResult> PushNotificationDlt()
    {
        await pubSubService.PushDltAsync(Request.Body, Request.Host.Value);
        return Ok("Notification received");
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