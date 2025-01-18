using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Entity;

namespace YoutubeApiSynchronize.Context;

public class MedreseDbContext : DbContext
{
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Video> Videos { get; set; }

    public DbSet<PlaylistVideo> PlaylistVideos { get; set; }
    
    public MedreseDbContext(DbContextOptions<MedreseDbContext> options) : base(options) { }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //      optionsBuilder.UseNpgsql("Host=31.220.95.127;Port=5433;Database=esmdb;Username=postgres;Password=123456789");
    //     // optionsBuilder.UseNpgsql(
    //     //     Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
    //     //     "Host=localhost;Port=5443;Database=newDb;Username=user2;Password=password2"
    //     // );
    // }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}