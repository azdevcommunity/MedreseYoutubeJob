using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.Dtos.Video.Requests;
using YoutubeApiSynchronize.Application.Dtos.Video.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Video;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/videos")]
public class VideoController : ControllerBase
{
    private readonly IVideoService _videoService;
    private readonly ILogger _logger;

    public VideoController(IVideoService videoService, ILogger logger)
    {
        _videoService = videoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<VideoResponse>>> GetAll(
        [FromQuery] int? page = null,
        [FromQuery] int? size = null,
        [FromQuery] string? search = null,
        [FromQuery] int shorts = 0,
        [FromQuery] string? playlistId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] int? maxResult = null)
    {
        // GET /api/videos?playlistId=X&page=Y&maxResult=Z
        if (!string.IsNullOrEmpty(playlistId) && page.HasValue && maxResult.HasValue)
        {
            _logger.Information("GetByPlaylistIdPaged endpoint called");
            var result = await _videoService.GetByPlaylistIdPagedAsync(playlistId, page.Value, maxResult.Value);
            return Ok(result);
        }

        // GET /api/videos?playlistId=X&sortBy=Y&sortOrder=Z
        if (!string.IsNullOrEmpty(playlistId) && !string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
        {
            _logger.Information("GetPlaylistVideosSorted endpoint called");
            var videos = await _videoService.GetByPlaylistIdSortedAsync(playlistId, sortBy, sortOrder);
            return Ok(videos);
        }

        // GET /api/videos?playlistId=X
        if (!string.IsNullOrEmpty(playlistId))
        {
            _logger.Information("GetByPlaylistId endpoint called with playlistId: {PlaylistId}", playlistId);
            var videos = await _videoService.GetByPlaylistIdAsync(playlistId);
            return Ok(videos);
        }

        // GET /api/videos?sortBy=X&sortOrder=Y
        if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
        {
            _logger.Information("GetAllSorted endpoint called");
            var videos = await _videoService.GetAllSortedAsync(sortBy, sortOrder);
            return Ok(videos);
        }

        // GET /api/videos?page=X&size=Y
        if (page.HasValue && size.HasValue)
        {
            _logger.Information("GetAllPaging endpoint called with page: {Page}, size: {Size}", page, size);
            var result = await _videoService.GetAllPagingAsync(
                page.Value, size.Value, search, shorts);
            return Ok(result);
        }

        // GET /api/videos (no parameters)
        _logger.Information("GetAll endpoint called");
        var allVideos = await _videoService.GetAllAsync();
        return Ok(allVideos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VideoResponse>> GetById(string id)
    {
        _logger.Information("GetById endpoint called with id: {Id}", id);
        var video = await _videoService.GetByIdAsync(id);
        return Ok(video);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateVideoRequest>> Update(
        string id,
        [FromBody] UpdateVideoRequest request)
    {
        _logger.Information("Update endpoint called with id: {Id}", id);
        var result = await _videoService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.Information("Delete endpoint called with id: {Id}", id);
        await _videoService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("latest")]
    public async Task<ActionResult<VideoResponse>> GetLatestVideo()
    {
        _logger.Information("GetLatestVideo endpoint called");
        var video = await _videoService.GetLatestVideoAsync();
        return Ok(video);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<VideoStatisticsResponse>> GetVideoStatistics()
    {
        _logger.Information("GetVideoStatistics endpoint called");
        var statistics = await _videoService.GetVideoStatisticsAsync();
        return Ok(statistics);
    }

    [HttpPut]
    public IActionResult UpdateVideos()
    {
        // Placeholder for future implementation
        _logger.Information("UpdateVideos endpoint called (not implemented)");
        return Ok(new { message = "Update videos endpoint - not implemented" });
    }
}