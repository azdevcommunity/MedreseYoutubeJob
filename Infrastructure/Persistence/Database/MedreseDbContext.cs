using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Core.Entities;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Database;

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
    public DbSet<Category> Categories { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ContactUs> ContactUs { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<ArticleCategory> ArticleCategories { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionCategory> QuestionCategories { get; set; }
    public DbSet<QuestionTag> QuestionTags { get; set; }
    public DbSet<Statistic> Statistics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Playlist>().HasQueryFilter(e => e.IsOldChannel == false);
        modelBuilder.Entity<Video>().HasQueryFilter(e => e.IsOldChannel == false);

        modelBuilder.Entity<YouTubeNotification>()
            .Property(n => n.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
