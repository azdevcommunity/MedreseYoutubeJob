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
        _logger.Information("GetAllAuthors endpoint called");
        var authors = await _authorService.GetAllAsync();
        return Ok(authors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorResponse>> GetAuthorById(int id)
    {
        _logger.Information("GetAuthorById endpoint called with id: {Id}", id);
        var author = await _authorService.GetByIdAsync(id);
        return Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorResponse>> CreateAuthor([FromBody] CreateAuthorRequest request)
    {
        _logger.Information("CreateAuthor endpoint called");
        var author = await _authorService.CreateAsync(request);
        return Ok(author);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AuthorResponse>> UpdateAuthor(int id, [FromBody] UpdateAuthorRequest request)
    {
        _logger.Information("UpdateAuthor endpoint called with id: {Id}", id);
        var author = await _authorService.UpdateAsync(id, request);
        return Ok(author);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        _logger.Information("DeleteAuthor endpoint called with id: {Id}", id);
        await _authorService.DeleteAsync(id);
        return NoContent();
    }
}
