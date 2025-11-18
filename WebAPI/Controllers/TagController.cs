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
        _logger.Information("GetAllTags endpoint called");
        var tags = await _tagService.GetAllAsync();
        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagResponse>> GetTagById(int id)
    {
        _logger.Information("GetTagById endpoint called with id: {Id}", id);
        var tag = await _tagService.GetByIdAsync(id);
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagResponse>> CreateTag([FromBody] CreateTagRequest request)
    {
        _logger.Information("CreateTag endpoint called");
        var tag = await _tagService.CreateAsync(request);
        return Ok(tag);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TagResponse>> UpdateTag(int id, [FromBody] UpdateTagRequest request)
    {
        _logger.Information("UpdateTag endpoint called with id: {Id}", id);
        var tag = await _tagService.UpdateAsync(id, request);
        return Ok(tag);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        _logger.Information("DeleteTag endpoint called with id: {Id}", id);
        await _tagService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("clear-cache")]
    public async Task<ActionResult<string>> ClearCache()
    {
        _logger.Information("ClearCache endpoint called");
        var result = await _tagService.ClearCacheAsync();
        return Ok(result);
    }
}