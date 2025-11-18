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
        _logger.Information("GetAllQuestions endpoint called with page: {Page}, size: {Size}", page, size);
        var questions = await _questionService.GetAllQuestionsAsync(
            page, size, tagIds, categoryIds, containsTag, containsCategory, search);
        return Ok(questions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionSearchResponse>> GetQuestionById(int id)
    {
        _logger.Information("GetQuestionById endpoint called with id: {Id}", id);
        var question = await _questionService.GetQuestionByIdAsync(id);
        return Ok(question);
    }

    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        _logger.Information("CreateQuestion endpoint called");
        var question = await _questionService.CreateQuestionAsync(request);
        return Ok(question);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Core.Entities.Question>> UpdateQuestion(
        int id,
        [FromBody] UpdateQuestionRequest request)
    {
        _logger.Information("UpdateQuestion endpoint called with id: {Id}", id);
        var question = await _questionService.UpdateQuestionAsync(id, request);
        return Ok(question);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        _logger.Information("DeleteQuestion endpoint called with id: {Id}", id);
        await _questionService.DeleteQuestionAsync(id);
        return NoContent();
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<QuestionStatisticsResponse>> GetQuestionStatistics()
    {
        _logger.Information("GetQuestionStatistics endpoint called");
        var statistics = await _questionService.GetQuestionStatisticsAsync();
        return Ok(statistics);
    }

    [HttpPut("{id}/increment-view")]
    public async Task<IActionResult> IncrementQuestionViewCount(int id)
    {
        _logger.Information("IncrementQuestionViewCount endpoint called with id: {Id}", id);
        await _questionService.IncrementQuestionViewCountAsync(id);
        return Ok();
    }

    [HttpGet("{id}/related")]
    public async Task<ActionResult<List<RelatedQuestionResponse>>> GetRelatedQuestions(int id)
    {
        _logger.Information("GetRelatedQuestions endpoint called with id: {Id}", id);
        var relatedQuestions = await _questionService.GetRelatedQuestionsAsync(id);
        return Ok(relatedQuestions);
    }
}