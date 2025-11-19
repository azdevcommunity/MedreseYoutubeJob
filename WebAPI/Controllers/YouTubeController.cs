using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.UseCases;
using YoutubeApiSynchronize.Core.Interfaces.Notification;
using YoutubeApiSynchronize.Core.Interfaces.Youtube;
using YoutubeApiSynchronize.Infrastructure.Persistence.Options;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class YouTubeController : ControllerBase
{
    private readonly IYouTubeService _youtubeService;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly SynchronizeYouTubeVideosUseCase _synchronizeUseCase;

    public YouTubeController(
        IYouTubeService youtubeService,
        INotificationService notificationService,
        IConfiguration configuration,
        ILogger logger,
        SynchronizeYouTubeVideosUseCase synchronizeUseCase)
    {
        _youtubeService = youtubeService;
        _notificationService = notificationService;
        _configuration = configuration;
        _logger = logger;
        _synchronizeUseCase = synchronizeUseCase;
    }

    [HttpGet("env")]
    public IActionResult GetEnv()
    {
        _logger.Information("GetEnv endpoint called");
        var db = _configuration.GetSection("DB").Get<DatabaseSettings>()!;

        _logger.Debug("Environment configuration retrieved successfully");
        return Ok(new
        {
            TestValue = _configuration["Test:Value"],
            db.ConnectionString,
        });
    }

    [HttpGet("/video-db/{videoId}")]
    public async Task<IActionResult> GetVideoFromDb(string videoId)
    {
        _logger.Information("Fetching video from database: {VideoId}", videoId);
        var result = await _youtubeService.GetVideoFromDbAsync(videoId);
        _logger.Debug("Video retrieved from database: {VideoId}", videoId);
        return Ok(result);
    }

    [HttpGet("/video-ytb/{videoId}")]
    public async Task<IActionResult> GetVideoFromYoutube(string videoId)
    {
        _logger.Information("Fetching video from YouTube: {VideoId}", videoId);
        var result = await _youtubeService.GetVideoFromYouTubeAsync(videoId);
        _logger.Debug("Video retrieved from YouTube: {VideoId}", videoId);
        return Ok(result);
    }

    [HttpPut("/{videoId}")]
    public async Task<IActionResult> UpdateByVideoId(string videoId)
    {
        _logger.Information("Updating video: {VideoId}", videoId);
        var result = await _youtubeService.UpdateVideoByIdAsync(videoId);
        _logger.Information("Video updated successfully: {VideoId}", videoId);
        return Ok(result);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncYouTubeData()
    {
        _logger.Information("YouTube synchronization started via API endpoint");
        var result = await _synchronizeUseCase.ExecuteAsync();
        _logger.Information("YouTube synchronization completed successfully");
        return Ok(result);
    }

    [HttpPost("update-channel-stats")]
    public async Task<IActionResult> UpdateChannelStats()
    {
        _logger.Information("Updating channel stats");
        var result = await _youtubeService.UpdateChannelStatsAsync();
        _logger.Information("Channel stats updated successfully");
        return Ok(result);
    }


    [HttpGet("channelstat")]
    public async Task<IActionResult> GetChannelStat()
    {
        _logger.Information("Channel statistics request received");
        var result = await _youtubeService.UpdateChannelStatsAsync();
        _logger.Information("Channel statistics retrieved successfully");
        return Ok(result);
    }

    [HttpPost("push")]
    public async Task<IActionResult> PushNotification()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        await _notificationService.ProcessNotificationAsync(payload);
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

        var notificationData = System.Text.Json.JsonSerializer.Serialize(new
        {
            hubMode,
            hubTopic,
            hubChallenge,
            hubLeaseSeconds,
            message = "Bu youtubedan gelir",
            host = Request.Host.Value
        });

        await _notificationService.ProcessNotificationAsync(notificationData, Request.Host.Value);
        return Ok("Notification received");
    }

    [HttpPost("push-dlt")]
    public async Task<IActionResult> PushNotificationDlt()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        await _notificationService.ProcessNotificationAsync(payload, Request.Host.Value);
        return Ok("Notification received");
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] string payload, [FromQuery] string challenge)
    {
        var channelId = _configuration.GetSection("YoutubeConfig:ChannelID").Value;
        var callbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/Youtube/push";

        await _notificationService.SubscribeToChannelAsync(channelId!, callbackUrl);
        return Ok("Success");
    }

    [HttpPost("UnsubscribeAndUpdateCallbackUrl")]
    public async Task<IActionResult> UnsubscribeAndUpdateCallbackUrl()
    {
        var channelId = _configuration.GetSection("YoutubeConfig:ChannelID").Value;
        var oldCallbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/youtube-pubsub/push";
        var newCallbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/Youtube/push";

        await _notificationService.UnsubscribeAndUpdateCallbackUrlAsync(oldCallbackUrl, newCallbackUrl, channelId!);
        return Ok("Subscription updated successfully.");
    }
}