using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Core.Entities;

namespace YoutubeApiSynchronize.Infrastructure.Database;

public class MedreseDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public MedreseDbContext(DbContextOptions<MedreseDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Video> Videos { get; set; }
    public DbSet<PlaylistVideo> PlaylistVideos { get; set; }
    public DbSet<ChannelStat> ChannelStats { get; set; }
    public DbSet<YouTubeNotification> YouTubeNotifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Playlist>().HasQueryFilter(e => e.IsOldChannel == false);
        modelBuilder.Entity<Video>().HasQueryFilter(e => e.IsOldChannel == false);

        modelBuilder.Entity<YouTubeNotification>()
            .Property(n => n.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
