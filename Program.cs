using System.Reflection;
using Google.Apis.Util;
using Serilog;
using YoutubeApiSynchronize.Configuration;
using YoutubeApiSynchronize.Jobs;
using YoutubeApiSynchronize.Options;
using YoutubeApiSynchronize.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "YoutubeApiSynchronize")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
});

Log.Information("Application starting... Environment: {Environment}", 
    builder.Environment.EnvironmentName);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<YoutubeService>();
builder.Services.Configure<YoutubeConfig>(builder.Configuration.GetSection("YoutubeConfig"));
builder.Services.Configure<LogOptions>(builder.Configuration.GetSection("LogConfig"));
builder.Services.AddHealthChecks();
builder.Services.Configure<ShortPlaylistsOptions>(
    builder.Configuration.GetSection(nameof(ShortPlaylistsOptions)));
builder.LoadConfiguration(new List<Assembly> { Assembly.GetExecutingAssembly() });

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseHealthChecks("/api/health");
app.UseCors("CorsPolicy");
app.Run();