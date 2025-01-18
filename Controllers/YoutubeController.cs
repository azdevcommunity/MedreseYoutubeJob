using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Services;

namespace YoutubeApiSynchronize.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class YoutubeController
    : ControllerBase
{
    private readonly YoutubeService _youtubeService;

    public YoutubeController(YoutubeService youtubeService)
    {
        _youtubeService = youtubeService;
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