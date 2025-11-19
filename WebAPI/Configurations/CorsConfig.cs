namespace YoutubeApiSynchronize.WebAPI.Configurations;

public static class CorsConfig
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            string[] corsOrigins = configuration.GetSection("CorsOrigins").Get<string[]>()
                                   ?? throw new NullReferenceException("CorsOrigins configuration is missing");
            
            options.AddPolicy(name: "CorsPolicy",
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        return services;
    }
}