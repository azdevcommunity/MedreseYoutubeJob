using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.Dtos.Statistic.Requests;
using YoutubeApiSynchronize.Application.Dtos.Statistic.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Statistic;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/statistics")]
public class StatisticController : ControllerBase
{
    private readonly IStatisticService _statisticService;
    private readonly ILogger _logger;

    public StatisticController(IStatisticService statisticService, ILogger logger)
    {
        _statisticService = statisticService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Core.Entities.Statistic>>> GetAllStatistics()
    {
        try
        {
            _logger.Information("GetAllStatistics endpoint called");
            var statistics = await _statisticService.GetAllStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetAllStatistics endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Core.Entities.Statistic>> GetStatisticById(int id)
    {
        try
        {
            _logger.Information("GetStatisticById endpoint called with id: {Id}", id);
            var statistic = await _statisticService.GetStatisticByIdAsync(id);
            return Ok(statistic);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Statistic not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetStatisticById endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<StatisticResponse>> CreateStatistic([FromBody] CreateStatisticRequest request)
    {
        try
        {
            _logger.Information("CreateStatistic endpoint called");
            var statistic = await _statisticService.CreateStatisticAsync(request);
            return Ok(statistic);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in CreateStatistic endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Core.Entities.Statistic>> UpdateStatistic(
        int id,
        [FromBody] UpdateStatisticRequest request)
    {
        try
        {
            _logger.Information("UpdateStatistic endpoint called with id: {Id}", id);
            var statistic = await _statisticService.UpdateStatisticAsync(id, request);
            return Ok(statistic);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Statistic not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in UpdateStatistic endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStatistic(int id)
    {
        try
        {
            _logger.Information("DeleteStatistic endpoint called with id: {Id}", id);
            await _statisticService.DeleteStatisticAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Statistic not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in DeleteStatistic endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
