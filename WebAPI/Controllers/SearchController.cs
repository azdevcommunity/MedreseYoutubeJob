using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.Dtos.Search.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Search;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger _logger;

    public SearchController(ISearchService searchService, ILogger logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<SearchResponse>> Search(
        [FromQuery] long? categoryId = null,
        [FromQuery] string? search = null)
    {
        _logger.Information("Search endpoint called with categoryId: {CategoryId}, search: {Search}",
            categoryId, search);
        var result = await _searchService.SearchAsync(categoryId, search);
        return Ok(result);
    }
}