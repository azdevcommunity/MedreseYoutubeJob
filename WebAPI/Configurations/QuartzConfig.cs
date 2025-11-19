using Quartz;
using YoutubeApiSynchronize.WebAPI.Jobs;

namespace YoutubeApiSynchronize.WebAPI.Configurations;

public static class QuartzConfig
{
    public static IServiceCollection AddQuartzConfiguration(this IServiceCollection services)
    {
        services.AddQuartz(cfg =>
        {
            var jobKey = new JobKey(nameof(YoutubeSynchronizeJob));

            cfg.AddJob<YoutubeSynchronizeJob>(opt => opt.WithIdentity(jobKey));

            cfg.AddTrigger(t => t
                .WithIdentity("YoutubeSyncTrigger")
                .ForJob(jobKey)
                .WithCronSchedule(
                    "0 0 22 * * ?",
                    x => x
                        .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"))
                        .WithMisfireHandlingInstructionDoNothing()
                )
            );
        });

        services.AddQuartzHostedService(opt => { opt.WaitForJobsToComplete = true; });

        return services;
    }
}