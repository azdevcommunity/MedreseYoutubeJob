using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.Dtos.Author.Requests;
using YoutubeApiSynchronize.Application.Dtos.Author.Responses;
using YoutubeApiSynchronize.Application.Services.Author;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/authors")]
public class AuthorController : ControllerBase
{
    private readonly IAuthorService _authorService;
    private readonly ILogger _logger;

    public AuthorController(IAuthorService authorService, ILogger logger)
    {
        _authorService = authorService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuthorResponse>>> GetAllAuthors()
    {
        try
        {
            _logger.Information("GetAllAuthors endpoint called");
            var authors = await _authorService.GetAllAsync();
            return Ok(authors);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetAllAuthors endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorResponse>> GetAuthorById(int id)
    {
        try
        {
            _logger.Information("GetAuthorById endpoint called with id: {Id}", id);
            var author = await _authorService.GetByIdAsync(id);
            return Ok(author);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Author not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetAuthorById endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AuthorResponse>> CreateAuthor([FromBody] CreateAuthorRequest request)
    {
        try
        {
            _logger.Information("CreateAuthor endpoint called");
            var author = await _authorService.CreateAsync(request);
            return Ok(author);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in CreateAuthor endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AuthorResponse>> UpdateAuthor(int id, [FromBody] UpdateAuthorRequest request)
    {
        try
        {
            _logger.Information("UpdateAuthor endpoint called with id: {Id}", id);
            var author = await _authorService.UpdateAsync(id, request);
            return Ok(author);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Author not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in UpdateAuthor endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        try
        {
            _logger.Information("DeleteAuthor endpoint called with id: {Id}", id);
            await _authorService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Author not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in DeleteAuthor endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
