using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using YoutubeApiSynchronize.Dto;
using YoutubeApiSynchronize.Options;
using YoutubeApiSynchronize.Services;

namespace YoutubeApiSynchronize.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class YoutubeController
    : ControllerBase
{
    private readonly YoutubeService _youtubeService;
    private readonly IConfiguration _configuration;

    public YoutubeController(YoutubeService youtubeService, IConfiguration configuration)
    {
        _youtubeService = youtubeService;
        _configuration = configuration;
    }

    [HttpGet("env")]
    public IActionResult GetEnv()
    {
        var db = _configuration.GetSection("DB").Get<DatabaseSettings>()!;

        return Ok(
            new
            {
                TestValue = _configuration["Test:Value"],
                db.ConnectionString,
            }
        );
    }

    [HttpGet("/video-db/{videoId}")]
    public async Task<IActionResult> GetVideoFromDb(string videoId)
    {
        return Ok(await _youtubeService.GetVideoFromDb(videoId));
    }

    [HttpGet("/video-ytb/{videoId}")]
    public async Task<IActionResult> GetVideoFromYoutube(string videoId)
    {
        return Ok(await _youtubeService.GetVideoFromYoutube(videoId));
    }


    [HttpPut("/{videoId}")]
    public async Task<IActionResult> UpdateByVideoId(string videoId)
    {
        return Ok(await _youtubeService.UpdateByVideoId(videoId));
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncYouTubeData()
    {
        try
        {
            return Ok(await _youtubeService.SyncAsync());
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(429, new { Message = ex.Message });
        }
    }


    [HttpPost("jenkins/{status}")]
    public IActionResult TurnJenkins(int status)
    {
        using var client = new SshClient(_configuration["SERVER_HOST"], _configuration["SERVER_USER_NAME"],
            _configuration["SERVER_PASSWORD"]);
        client.Connect();

        if (status == 1)
        {
            Console.WriteLine("Starting Jenkins...");
            client.RunCommand("sudo systemctl start jenkins");
        }
        else if (status == 0)
        {
            Console.WriteLine("Stopping Jenkins...");
            client.RunCommand("sudo systemctl stop jenkins");
        }
        else
        {
            return BadRequest("Geçersiz parametre. Sadece 0 veya 1 olmalı.");
        }

        client.Disconnect();
        return Ok(new { message = $"Jenkins {(status == 1 ? "başlatıldı" : "durduruldu")}" });
    }

    [HttpGet("channelstat")]
    public async Task<object> GetChannelStat()
    {
        return await _youtubeService.UpdateChannelStatsAsync();
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string xmlContent)
    {
        try
        {
            var xmlDoc = XDocument.Parse(xmlContent);

            var entry = xmlDoc.Descendants("{http://www.w3.org/2005/Atom}entry").FirstOrDefault();
            if (entry == null)
            {
                return BadRequest("Invalid notification format.");
            }

            var notification = new YouTubeNotificationDto
            {
                VideoId = entry.Element("{http://www.youtube.com/xml/schemas/2015}videoId")?.Value,
                ChannelId = entry.Element("{http://www.youtube.com/xml/schemas/2015}channelId")?.Value,
                Title = entry.Element("{http://www.w3.org/2005/Atom}title")?.Value,
                VideoUrl = entry.Element("{http://www.w3.org/2005/Atom}link")?.Attribute("href")?.Value,
                ChannelUrl = entry.Descendants("{http://www.w3.org/2005/Atom}author")
                    .Descendants("{http://www.w3.org/2005/Atom}uri").FirstOrDefault()?.Value,
                Published = entry.Element("{http://www.w3.org/2005/Atom}published")?.Value,
                Updated = entry.Element("{http://www.w3.org/2005/Atom}updated")?.Value
            };

            Console.WriteLine($"Video ID: {notification.VideoId}");
            Console.WriteLine($"Channel ID: {notification.ChannelId}");
            Console.WriteLine($"Title: {notification.Title}");
            Console.WriteLine($"Video URL: {notification.VideoUrl}");
            Console.WriteLine($"Channel URL: {notification.ChannelUrl}");
            Console.WriteLine($"Published: {notification.Published}");
            Console.WriteLine($"Updated: {notification.Updated}");


            return Ok("Notification processed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing notification: {ex.Message}");
            return BadRequest("Error processing notification");
        }
    }
}