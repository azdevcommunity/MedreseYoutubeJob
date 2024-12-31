using YoutubeApiSyncronize.Context;
using YoutubeApiSyncronize.Jobs;
using YoutubeApiSyncronize.Options;
using YoutubeApiSyncronize.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MedreseDbContext>();
builder.Services.AddScoped<YoutubeService>();
builder.Services.AddHostedService<YoutubeSynchronize>();
builder.Services.Configure<YoutubeConfig>(builder.Configuration.GetSection("YoutubeConfig"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();