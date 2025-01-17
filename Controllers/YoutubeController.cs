using Microsoft.AspNetCore.Mvc;
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
                ConnectionString =
                    $"Host={db.Host};Port={db.Port};Database={db.Name};Username={db.Username};Password={db.Password}"
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
}