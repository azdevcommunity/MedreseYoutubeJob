using YoutubeApiSynchronize.Core.Interfaces;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.Application.UseCases;

public class SynchronizeYouTubeVideosUseCase
{
    private readonly IYouTubeService _youTubeService;
    private readonly ILogger _logger;

    public SynchronizeYouTubeVideosUseCase(IYouTubeService youTubeService, ILogger logger)
    {
        _youTubeService = youTubeService;
        _logger = logger;
    }

    public async Task<object> ExecuteAsync()
    {
        try
        {
            _logger.Information("Executing YouTube synchronization use case");
            
            await _youTubeService.SyncAsync();
            await _youTubeService.UpdateChannelStatsAsync();
            
            _logger.Information("YouTube synchronization use case completed successfully");
            return new { Success = true, Message = "Synchronization completed successfully" };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error executing YouTube synchronization use case");
            return new { Success = false, Message = "Synchronization failed", Error = ex.Message };
        }
    }
}
