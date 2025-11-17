using YoutubeApiSynchronize.Attributes;

namespace YoutubeApiSynchronize.Configuration;

[Configuration]
public class CorsConfig
{
    public void AddCors(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            string[] corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
                                   ?? throw new NullReferenceException();
            options.AddPolicy(name: "CorsPolicy",
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
    }
}