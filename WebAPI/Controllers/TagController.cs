using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.Dtos.Tag.Requests;
using YoutubeApiSynchronize.Application.Dtos.Tag.Responses;
using YoutubeApiSynchronize.Application.Services.Tag;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger _logger;

    public TagController(ITagService tagService, ILogger logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<TagResponse>>> GetAllTags()
    {
        try
        {
            _logger.Information("GetAllTags endpoint called");
            var tags = await _tagService.GetAllAsync();
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetAllTags endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagResponse>> GetTagById(int id)
    {
        try
        {
            _logger.Information("GetTagById endpoint called with id: {Id}", id);
            var tag = await _tagService.GetByIdAsync(id);
            return Ok(tag);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Tag not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetTagById endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<TagResponse>> CreateTag([FromBody] CreateTagRequest request)
    {
        try
        {
            _logger.Information("CreateTag endpoint called");
            var tag = await _tagService.CreateAsync(request);
            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in CreateTag endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TagResponse>> UpdateTag(int id, [FromBody] UpdateTagRequest request)
    {
        try
        {
            _logger.Information("UpdateTag endpoint called with id: {Id}", id);
            var tag = await _tagService.UpdateAsync(id, request);
            return Ok(tag);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Tag not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in UpdateTag endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        try
        {
            _logger.Information("DeleteTag endpoint called with id: {Id}", id);
            await _tagService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Tag not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in DeleteTag endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("clear-cache")]
    public async Task<ActionResult<string>> ClearCache()
    {
        try
        {
            _logger.Information("ClearCache endpoint called");
            var result = await _tagService.ClearCacheAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in ClearCache endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
