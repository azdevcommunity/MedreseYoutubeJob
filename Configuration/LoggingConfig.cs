using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using YoutubeApiSynchronize.Attributes;
using YoutubeApiSynchronize.Options;

namespace YoutubeApiSynchronize.Configuration;

[Configuration]
public class LoggingConfig(IOptions<LogOptions> logOptions)
{
    public void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, serilogServices, configuration) =>
        {
            var env = builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            var hostname = builder.Configuration["HOSTNAME"] ?? "localhost";
            configuration
                // .Enrich.WithProperty("app", "youtube-job-api")
                // .Enrich.WithProperty("env", env)
                // .Enrich.WithProperty("host", hostname)
                .WriteTo.Console()
                // .WriteTo.GrafanaLoki(logOptions.Value.Loki,
                // [
                //     new LokiLabel
                //     {
                //         Key = "app",
                //         Value = "youtube-job-api"
                //     },
                //     new LokiLabel
                //     {
                //         Key = "env",
                //         Value = env
                //     },
                //     new LokiLabel
                //     {
                //         Key = "host",
                //         Value = hostname
                //     }
                // ])
                .ReadFrom.Services(serilogServices)
                .Enrich.FromLogContext();
        });
    }
}