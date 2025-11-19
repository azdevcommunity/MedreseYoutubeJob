using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Core.Entities;
using YoutubeApiSynchronize.Core.Interfaces;
using YoutubeApiSynchronize.Core.Interfaces.Youtube;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

public class YouTubeRepository : IYouTubeRepository
{
    private readonly MedreseDbContext _context;

    public YouTubeRepository(MedreseDbContext context)
    {
        _context = context;
    }

    public async Task<Video?> GetVideoByIdAsync(string videoId)
    {
        return await _context.Videos.FirstOrDefaultAsync(x => x.VideoId == videoId);
    }

    public async Task<Playlist?> GetPlaylistByIdAsync(string playlistId)
    {
        return await _context.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId);
    }

    public async Task<ChannelStat?> GetChannelStatAsync()
    {
        return await _context.ChannelStats.AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task<bool> VideoExistsAsync(string videoId)
    {
        return await _context.Videos.AnyAsync(v => v.VideoId == videoId);
    }

    public async Task<bool> PlaylistExistsAsync(string playlistId)
    {
        return await _context.Playlists.AnyAsync(p => p.PlaylistId == playlistId);
    }

    public async Task<bool> PlaylistVideoExistsAsync(string videoId, string playlistId)
    {
        return await _context.PlaylistVideos.AnyAsync(v => v.VideoId == videoId && v.PlaylistId == playlistId);
    }

    public async Task<int> GetPlaylistVideoCountAsync(string playlistId)
    {
        return await _context.PlaylistVideos.CountAsync(x => x.PlaylistId == playlistId);
    }

    public async Task AddVideoAsync(Video video)
    {
        await _context.Videos.AddAsync(video);
    }

    public async Task AddPlaylistAsync(Playlist playlist)
    {
        await _context.Playlists.AddAsync(playlist);
    }

    public async Task AddPlaylistVideoAsync(PlaylistVideo playlistVideo)
    {
        await _context.PlaylistVideos.AddAsync(playlistVideo);
    }

    public Task UpdateVideoAsync(Video video)
    {
        _context.Videos.Update(video);
        return Task.CompletedTask;
    }

    public Task UpdatePlaylistAsync(Playlist playlist)
    {
        _context.Playlists.Update(playlist);
        return Task.CompletedTask;
    }

    public Task UpdateChannelStatAsync(ChannelStat channelStat)
    {
        _context.ChannelStats.Update(channelStat);
        return Task.CompletedTask;
    }

    public async Task UpdatePlaylistVideoCountsAsync()
    {
        var playlists = await _context.Playlists.ToListAsync();

        foreach (var playlist in playlists)
        {
            var count = await GetPlaylistVideoCountAsync(playlist.PlaylistId);
            playlist.VideoCount = count;
        }

        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
