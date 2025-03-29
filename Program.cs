using System.Reflection;
using YoutubeApiSynchronize.Configuration;
using YoutubeApiSynchronize.Jobs;
using YoutubeApiSynchronize.Options;
using YoutubeApiSynchronize.Services;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<YoutubeService>();
// builder.Services.AddHostedService<YoutubeSynchronize>();
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
app.Run();