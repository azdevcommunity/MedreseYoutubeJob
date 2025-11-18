using Microsoft.Extensions.DependencyInjection;
using Quartz;
using YoutubeApiSynchronize.Application.UseCases;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Jobs;

[DisallowConcurrentExecution]
public class YoutubeSynchronizeJob(IServiceScopeFactory serviceScopeFactory, ILogger logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.Information("Synchronizing job starting...");

        using var scope = serviceScopeFactory.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<SynchronizeYouTubeVideosUseCase>();

        try
        {
            await useCase.ExecuteAsync();
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