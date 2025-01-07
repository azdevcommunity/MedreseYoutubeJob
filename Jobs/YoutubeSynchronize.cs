using YoutubeApiSyncronize.Services;

namespace YoutubeApiSyncronize.Jobs;

public class YoutubeSynchronize(IServiceScopeFactory scopeFactory, ILogger<YoutubeSynchronize> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = CalculateDuration();
                await Task.Delay(delay, stoppingToken);
                await SyncAsync();
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError("Cancelling background job, {Message}", ex.Message);
            }
        }
    }

    private TimeSpan CalculateDuration()
    {
        var now = DateTime.Now;
        var nextRunTime = DateTime.Today.AddHours(22);
        if (now > nextRunTime)
        {
            nextRunTime = nextRunTime.AddDays(1);
        }

        var delay = nextRunTime - now;

        return delay;
    }

    private async Task SyncAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<YoutubeService>();
        try
        {
            await service.SyncAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Synchronization failed: {Message}", ex.Message);
        }
    }
}