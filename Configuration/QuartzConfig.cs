using Quartz;
using YoutubeApiSynchronize.Attributes;
using YoutubeApiSynchronize.Jobs;

namespace YoutubeApiSynchronize.Configuration;

[Configuration]
public class QuartzConfig
{
    public void AddCors(WebApplicationBuilder builder)
    {
        builder.Services.AddQuartz(cfg =>
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

        builder.Services.AddQuartzHostedService(opt => { opt.WaitForJobsToComplete = true; });
    }
}