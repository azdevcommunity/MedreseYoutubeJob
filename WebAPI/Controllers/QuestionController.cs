using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.Dtos.Question.Requests;
using YoutubeApiSynchronize.Application.Dtos.Question.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Question;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/questions")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly ILogger _logger;

    public QuestionController(IQuestionService questionService, ILogger logger)
    {
        _questionService = questionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedQuestionResponse>> GetAllQuestions(
        [FromQuery] int page = 0,
        [FromQuery(Name = "maxResult")] int size = 10,
        [FromQuery(Name = "tagIds")] List<int>? tagIds = null,
        [FromQuery(Name = "categoryIds")] List<int>? categoryIds = null,
        [FromQuery(Name = "containsTag")] int containsTag = 0,
        [FromQuery(Name = "searchQuery")] string? search = null,
        [FromQuery(Name = "containsCategory")] int containsCategory = 0)
    {
        try
        {
            _logger.Information("GetAllQuestions endpoint called with page: {Page}, size: {Size}", page, size);
            var questions = await _questionService.GetAllQuestionsAsync(
                page, size, tagIds, categoryIds, containsTag, containsCategory, search);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetAllQuestions endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionSearchResponse>> GetQuestionById(int id)
    {
        try
        {
            _logger.Information("GetQuestionById endpoint called with id: {Id}", id);
            var question = await _questionService.GetQuestionByIdAsync(id);
            return Ok(question);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Question not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetQuestionById endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        try
        {
            _logger.Information("CreateQuestion endpoint called");
            var question = await _questionService.CreateQuestionAsync(request);
            return Ok(question);
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warning(ex, "Invalid operation in CreateQuestion");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in CreateQuestion endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Core.Entities.Question>> UpdateQuestion(
        int id,
        [FromBody] UpdateQuestionRequest request)
    {
        try
        {
            _logger.Information("UpdateQuestion endpoint called with id: {Id}", id);
            var question = await _questionService.UpdateQuestionAsync(id, request);
            return Ok(question);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Question not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warning(ex, "Invalid operation in UpdateQuestion");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in UpdateQuestion endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        try
        {
            _logger.Information("DeleteQuestion endpoint called with id: {Id}", id);
            await _questionService.DeleteQuestionAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Question not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in DeleteQuestion endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<QuestionStatisticsResponse>> GetQuestionStatistics()
    {
        try
        {
            _logger.Information("GetQuestionStatistics endpoint called");
            var statistics = await _questionService.GetQuestionStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetQuestionStatistics endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("{id}/increment-view")]
    public async Task<IActionResult> IncrementQuestionViewCount(int id)
    {
        try
        {
            _logger.Information("IncrementQuestionViewCount endpoint called with id: {Id}", id);
            await _questionService.IncrementQuestionViewCountAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Question not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in IncrementQuestionViewCount endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}/related")]
    public async Task<ActionResult<List<RelatedQuestionResponse>>> GetRelatedQuestions(int id)
    {
        try
        {
            _logger.Information("GetRelatedQuestions endpoint called with id: {Id}", id);
            var relatedQuestions = await _questionService.GetRelatedQuestionsAsync(id);
            return Ok(relatedQuestions);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Question not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetRelatedQuestions endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
