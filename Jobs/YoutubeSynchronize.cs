using YoutubeApiSyncronize.Services;

namespace YoutubeApiSyncronize.Jobs;

public class YoutubeSynchronize : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public YoutubeSynchronize(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRunTime = DateTime.Today.AddHours(22); 
            if (now > nextRunTime)
            {
                nextRunTime = nextRunTime.AddDays(1);
            }

            var delay = nextRunTime - now;

            await Task.Delay(delay, stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<YoutubeService>();
            try
            {
                await service.SyncAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Synchronization skipped: {ex.Message}");
            }
        }
    }
}