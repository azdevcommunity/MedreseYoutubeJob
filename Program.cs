using System.Net.Mime;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Serilog;
using YoutubeApiSynchronize.Context;
using YoutubeApiSynchronize.Jobs;
using YoutubeApiSynchronize.Options;
using YoutubeApiSynchronize.Services;
using YoutubeApiSynchronize.Util;

var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MedreseDbContext>(options =>
{
    var db = builder.Configuration.GetSection("DB").Get<DatabaseSettings>()!;
    options.UseNpgsql(
        $"Host={db.Host};Port={db.Port};Database={db.Name};Username={db.Username};Password={db.Password}"
    );
});
builder.Services.AddScoped<YoutubeService>();
builder.Services.AddHostedService<YoutubeSynchronize>();
builder.Services.Configure<YoutubeConfig>(builder.Configuration.GetSection("YoutubeConfig"));
builder.Services.AddHealthChecks();
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.None;
    logging.RequestHeaders.Add("sec-ch-ua");
    logging.MediaTypeOptions.AddText(MediaTypeNames.Application.Json);
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

builder.Host.UseSerilog((context, serilogServices, configuration) =>
{
    configuration
        .WriteTo.Console()
        .ReadFrom.Services(serilogServices)
        .Enrich.FromLogContext();
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