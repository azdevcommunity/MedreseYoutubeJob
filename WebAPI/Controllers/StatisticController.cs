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
        _logger.Information("GetAllStatistics endpoint called");
        var statistics = await _statisticService.GetAllStatisticsAsync();
        return Ok(statistics);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Core.Entities.Statistic>> GetStatisticById(int id)
    {
        _logger.Information("GetStatisticById endpoint called with id: {Id}", id);
        var statistic = await _statisticService.GetStatisticByIdAsync(id);
        return Ok(statistic);
    }

    [HttpPost]
    public async Task<ActionResult<StatisticResponse>> CreateStatistic([FromBody] CreateStatisticRequest request)
    {
        _logger.Information("CreateStatistic endpoint called");
        var statistic = await _statisticService.CreateStatisticAsync(request);
        return Ok(statistic);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Core.Entities.Statistic>> UpdateStatistic(
        int id,
        [FromBody] UpdateStatisticRequest request)
    {
        _logger.Information("UpdateStatistic endpoint called with id: {Id}", id);
        var statistic = await _statisticService.UpdateStatisticAsync(id, request);
        return Ok(statistic);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStatistic(int id)
    {
        _logger.Information("DeleteStatistic endpoint called with id: {Id}", id);
        await _statisticService.DeleteStatisticAsync(id);
        return NoContent();
    }
}