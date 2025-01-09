using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using YoutubeApiSyncronize.Context;
using YoutubeApiSyncronize.Jobs;
using YoutubeApiSyncronize.Options;
using YoutubeApiSyncronize.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MedreseDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration["ConnectionString"]);
});
builder.Services.AddScoped<YoutubeService>();
builder.Services.AddHostedService<YoutubeSynchronize>();
builder.Services.Configure<YoutubeConfig>(builder.Configuration.GetSection("YoutubeConfig"));
builder.Services.AddHealthChecks();
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestHeaders.Add("Authorization");
    logging.ResponseHeaders.Add("Content-Type");
    logging.MediaTypeOptions.AddText(MediaTypeNames.Application.Json);
    logging.MediaTypeOptions.AddText(MediaTypeNames.Application.Xml);
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpLogging();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseHealthChecks("/api/health");
app.Run();