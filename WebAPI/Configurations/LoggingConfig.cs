using Serilog;
using Serilog.Sinks.Grafana.Loki;
using YoutubeApiSynchronize.Options;

namespace YoutubeApiSynchronize.WebAPI.Configurations;

public static class LoggingConfig
{
    public static WebApplicationBuilder AddLoggingConfiguration(this WebApplicationBuilder builder)
    {
        
        // Add Serilog configuration from separate file
        builder.Configuration
            .AddJsonFile("serilog.json", optional: false, reloadOnChange: true);

        
        builder.Host.UseSerilog((context, serilogServices, configuration) =>
        {
            var env = context.HostingEnvironment.EnvironmentName;
            var hostname = builder.Configuration["HOSTNAME"] ?? "localhost";
            
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(serilogServices)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "YoutubeApiSynchronize")
                .Enrich.WithProperty("Environment", env)
                .Enrich.WithProperty("Hostname", hostname);

            var logConfig = builder.Configuration.GetSection(nameof(LogConfig)).Get<LogConfig>();
            if (logConfig != null && !string.IsNullOrEmpty(logConfig.Loki))
            {
                configuration.WriteTo.GrafanaLoki(logConfig.Loki,
                [
                    new LokiLabel { Key = "app", Value = "youtube-job-api" },
                    new LokiLabel { Key = "env", Value = env },
                    new LokiLabel { Key = "host", Value = hostname }
                ]);
            }
        });
        
        builder.Services.AddHttpLogging(options =>
        {
            options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
            options.RequestHeaders.Add("Content-Type");
            options.RequestHeaders.Add("User-Agent");
            options.RequestHeaders.Add("X-Forwarded-For");
    
            options.ResponseHeaders.Add("Content-Type");
            options.ResponseHeaders.Add("Content-Length");
            options.ResponseHeaders.Add("Server");
    
            options.MediaTypeOptions.AddText("application/json");
            options.MediaTypeOptions.AddText("application/xml");
            options.MediaTypeOptions.AddText("text/plain");
        });

        return builder;
    }
}
