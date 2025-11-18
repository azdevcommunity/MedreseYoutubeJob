using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.Dtos.Article.Requests;
using YoutubeApiSynchronize.Application.Dtos.Article.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Article;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/articles")]
public class ArticleController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly ILogger _logger;

    public ArticleController(IArticleService articleService, ILogger logger)
    {
        _articleService = articleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedArticleResponse>> GetAllArticles(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] List<long>? categoryIds = null)
    {
        try
        {
            _logger.Information("GetAllArticles endpoint called with page: {Page}, size: {Size}", page, size);
            var articles = await _articleService.GetAllArticlesAsync(page, size, categoryIds);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetAllArticles endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<ArticleIdResponse>>> GetAllArticleIds()
    {
        try
        {
            _logger.Information("GetAllArticleIds endpoint called");
            var articles = await _articleService.GetAllArticleIdsAsync();
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetAllArticleIds endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArticleResponse>> GetArticle(
        int id,
        [FromHeader(Name = "X-Admin-Request")] bool isAdminRequest = false)
    {
        try
        {
            _logger.Information("GetArticle endpoint called with id: {Id}, isAdminRequest: {IsAdminRequest}", 
                id, isAdminRequest);
            var article = await _articleService.GetArticleByIdAsync(id, isAdminRequest);
            return Ok(article);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Article not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetArticle endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("popular")]
    public async Task<ActionResult<List<PopularArticleResponse>>> GetPopularArticles()
    {
        try
        {
            _logger.Information("GetPopularArticles endpoint called");
            var articles = await _articleService.GetPopularArticlesAsync();
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetPopularArticles endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ArticleStatisticsResponse>> GetArticleStatistics()
    {
        try
        {
            _logger.Information("GetArticleStatistics endpoint called");
            var statistics = await _articleService.GetArticleStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetArticleStatistics endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("count/{id}")]
    public async Task<IActionResult> IncrementReadCount(int id)
    {
        try
        {
            _logger.Information("IncrementReadCount endpoint called with id: {Id}", id);
            await _articleService.IncrementReadCountAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in IncrementReadCount endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ArticleResponse>> CreateArticle([FromBody] CreateArticleRequest request)
    {
        try
        {
            _logger.Information("CreateArticle endpoint called");
            var article = await _articleService.CreateArticleAsync(request);
            return Ok(article);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Resource not found in CreateArticle");
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warning(ex, "Invalid operation in CreateArticle");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in CreateArticle endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Core.Entities.Article>> UpdateArticle(
        int id, 
        [FromBody] UpdateArticleRequest request)
    {
        try
        {
            _logger.Information("UpdateArticle endpoint called with id: {Id}", id);
            var article = await _articleService.UpdateArticleAsync(id, request);
            return Ok(article);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Resource not found in UpdateArticle with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warning(ex, "Invalid operation in UpdateArticle");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in UpdateArticle endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        try
        {
            _logger.Information("DeleteArticle endpoint called with id: {Id}", id);
            await _articleService.DeleteArticleAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Article not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in DeleteArticle endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteArticles([FromBody] DeleteArticlesRequest request)
    {
        try
        {
            _logger.Information("DeleteArticles endpoint called with {Count} ids", request.Ids.Count);
            await _articleService.DeleteArticlesAsync(request);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in DeleteArticles endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
