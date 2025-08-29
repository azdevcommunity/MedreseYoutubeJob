using YoutubeApiSynchronize.Services;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.Jobs;

using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;

[DisallowConcurrentExecution]
public class YoutubeSynchronizeJob(IServiceProvider sp, ILogger logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.Information("Synchronizing job starting ...");

        using var scope = sp.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<YoutubeService>();

        try
        {
            await service.SyncAsync();
            await service.UpdateChannelStatsAsync();
            logger.Information("Synchronization finished.");
        }
        catch (OperationCanceledException ex) when (context.CancellationToken.IsCancellationRequested)
        {
            logger.Warning(ex, "Job cancelled: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Synchronization failed: {Message}", ex.Message);
        }
    }
}