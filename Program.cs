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


builder.Services.AddHttpLogging(options =>
{
    // Nəyi loglamaq istədiyini təyin et
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    // Request headers
    options.RequestHeaders.Add("Content-Type");
    options.RequestHeaders.Add("User-Agent");
    options.RequestHeaders.Add("X-Forwarded-For");
    
    // Response headers
    options.ResponseHeaders.Add("Content-Type");
    options.ResponseHeaders.Add("Content-Length");
    options.ResponseHeaders.Add("Server");
    
    // Media types
    options.MediaTypeOptions.AddText("application/json");
    options.MediaTypeOptions.AddText("application/xml");
    options.MediaTypeOptions.AddText("text/plain");
});
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