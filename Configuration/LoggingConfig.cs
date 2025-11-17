using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using YoutubeApiSynchronize.Attributes;
using YoutubeApiSynchronize.Options;

namespace YoutubeApiSynchronize.Configuration;

/// <summary>
/// Serilog logging configuration for the application.
/// Supports console output, file logging, and optional Grafana Loki integration.
/// </summary>
[Configuration]
public class LoggingConfig(IOptions<LogOptions> logOptions)
{
    /// <summary>
    /// Configures Serilog for the application with console and file sinks.
    /// </summary>
    public void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, serilogServices, configuration) =>
        {
            var env = context.HostingEnvironment.EnvironmentName;
            var hostname = builder.Configuration["HOSTNAME"] ?? "localhost";
            
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(serilogServices)
                .Enrich.FromLogContext()
                //.Enrich.WithMachineName()
                // .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "YoutubeApiSynchronize")
                .Enrich.WithProperty("Environment", env)
                .Enrich.WithProperty("Hostname", hostname);

            // Optional: Uncomment to enable Grafana Loki integration
            // if (!string.IsNullOrEmpty(logOptions.Value.Loki))
            // {
            //     configuration.WriteTo.GrafanaLoki(logOptions.Value.Loki,
            //     [
            //         new LokiLabel { Key = "app", Value = "youtube-job-api" },
            //         new LokiLabel { Key = "env", Value = env },
            //         new LokiLabel { Key = "host", Value = hostname }
            //     ]);
            // }
        });
    }
}