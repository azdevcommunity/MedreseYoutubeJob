using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.Dtos.Playlist.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Playlist;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/playlists")]
public class PlaylistController : ControllerBase
{
    private readonly IPlaylistService _playlistService;
    private readonly ILogger _logger;

    public PlaylistController(IPlaylistService playlistService, ILogger logger)
    {
        _playlistService = playlistService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlaylistResponse>>> GetAllPlaylists(
        [FromQuery] string? search = null)
    {
        try
        {
            _logger.Information("GetAllPlaylists endpoint called with search: {Search}", search);
            var playlists = await _playlistService.GetAllPlaylistsWithSearchAsync(search);
            return Ok(playlists);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetAllPlaylists endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("of-video/{videoId}")]
    public async Task<ActionResult<PlaylistResponse>> GetByOfVideo(string videoId)
    {
        try
        {
            _logger.Information("GetByOfVideo endpoint called with videoId: {VideoId}", videoId);
            var playlist = await _playlistService.GetByOfVideoAsync(videoId);
            return Ok(playlist);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Playlist not found for video id: {VideoId}", videoId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetByOfVideo endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{playlistId}")]
    public async Task<ActionResult<PlaylistResponse>> GetPlaylistById(string playlistId)
    {
        try
        {
            _logger.Information("GetPlaylistById endpoint called with playlistId: {PlaylistId}", playlistId);
            var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
            return Ok(playlist);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Playlist not found with id: {PlaylistId}", playlistId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetPlaylistById endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
