using YoutubeApiSynchronize.Attributes;

namespace YoutubeApiSynchronize.Configuration;

[Configuration]
public class CorsConfig
{
    public void AddCors(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "CorsPolicy",
                policy =>
                {
                    policy.WithOrigins(builder.Configuration["CorsOrigins"]!)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
    }
}