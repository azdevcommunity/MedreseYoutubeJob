using System.Reflection;
using YoutubeApiSynchronize.Configuration;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

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