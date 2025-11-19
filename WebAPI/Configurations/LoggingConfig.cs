using System.Net;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using YoutubeApiSynchronize.Core.Options;

namespace YoutubeApiSynchronize.WebAPI.Configurations;

public static class LoggingConfig
{
    public static WebApplicationBuilder AddLoggingConfiguration(this WebApplicationBuilder builder)
    {
        Console.WriteLine("=== Configuration Values ===");
        Console.WriteLine($"DB__Host: {builder.Configuration["DB__Host"]}");
        Console.WriteLine($"DB__Port: {builder.Configuration["DB__Port"]}");
        Console.WriteLine($"DB__Name: {builder.Configuration["DB__Name"]}");
        Console.WriteLine($"DB__Username: {builder.Configuration["DB__Username"]}");
        Console.WriteLine($"DB__Password: {builder.Configuration["DB__Password"]}");
        
        builder.Configuration
            .AddJsonFile("serilog.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"serilog.{builder.Configuration["ASPNETCORE_ENVIRONMENT"]}.json", optional: true,
                reloadOnChange: true);

        builder.Host.UseSerilog((context, services, config) =>
        {
            string env = context.HostingEnvironment.EnvironmentName;
            string hostname = builder.Configuration["HOSTNAME"] ?? Dns.GetHostName();

            config
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "YoutubeApiSynchronize")
                .Enrich.WithProperty("Environment", env)
                .Enrich.WithProperty("Hostname", hostname)
                .WriteTo.Seq(builder.Configuration["SeqUrl"]!);
        });


        return builder;
    }
}