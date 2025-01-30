using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YoutubeApiSynchronize.Attributes;
using YoutubeApiSynchronize.Context;
using YoutubeApiSynchronize.Options;

namespace YoutubeApiSynchronize.Configuration;

[Configuration]
public class DatabaseConfig
{
    
    public void AddDatabase(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<MedreseDbContext>(options =>
        {
            var db = builder.Configuration.GetSection("DB").Get<DatabaseSettings>()!;
            options.UseNpgsql(
                $"Host={db.Host};Port={db.Port};Database={db.Name};Username={db.Username};Password={db.Password}"
            );
        });
    }
}