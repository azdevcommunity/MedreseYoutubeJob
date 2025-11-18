using YoutubeApiSynchronize.WebAPI.Configurations;
using YoutubeApiSynchronize.WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging
builder.AddLoggingConfiguration();

// Add Controllers and API Documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Add Clean Architecture Services (Infrastructure + Application + WebAPI)
builder.Services.AddWebApiServices(builder.Configuration);

var app = builder.Build();

// Add Global Error Handling Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure HTTP Pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseHealthChecks("/api/health");
app.UseCors("CorsPolicy");

app.Run();
