using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Application.Dtos.Playlist.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Playlist;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly MedreseDbContext _context;

    public PlaylistRepository(MedreseDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlaylistResponse>> GetAllWithSearchAsync(string? search)
    {
        var query = _context.Playlists.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Title.Contains(search));
        }

        // Get playlists with their latest video's published date for ordering
        var playlists = await query
            .GroupJoin(
                _context.PlaylistVideos,
                p => p.PlaylistId,
                pv => pv.PlaylistId,
                (p, pvs) => new { Playlist = p, PlaylistVideos = pvs })
            .SelectMany(
                x => x.PlaylistVideos.DefaultIfEmpty(),
                (x, pv) => new { x.Playlist, PlaylistVideo = pv })
            .GroupJoin(
                _context.Videos,
                x => x.PlaylistVideo != null ? x.PlaylistVideo.VideoId : null,
                v => v.VideoId,
                (x, vs) => new { x.Playlist, Video = vs.FirstOrDefault() })
            .GroupBy(x => new
            {
                x.Playlist.Id,
                x.Playlist.PlaylistId,
                x.Playlist.Title,
                x.Playlist.PublishedAt,
                x.Playlist.Thumbnail,
                x.Playlist.VideoCount,
                x.Playlist.IsOldChannel
            })
            .Select(g => new
            {
                Playlist = g.Key,
                LatestVideoDate = g.Max(x => x.Video != null ? x.Video.PublishedAt : null)
            })
            .OrderByDescending(x => x.LatestVideoDate ?? x.Playlist.PublishedAt)
            .Select(x => new PlaylistResponse
            {
                Id = x.Playlist.Id,
                PlaylistId = x.Playlist.PlaylistId,
                Title = x.Playlist.Title,
                PublishedAt = x.Playlist.PublishedAt,
                Thumbnail = x.Playlist.Thumbnail,
                VideoCount = x.Playlist.VideoCount,
                IsOldChannel = x.Playlist.IsOldChannel
            })
            .ToListAsync();

        return playlists;
    }

    public async Task<PlaylistResponse?> GetByPlaylistIdAsync(string playlistId)
    {
        return await _context.Playlists
            .Where(p => p.PlaylistId == playlistId)
            .Select(p => new PlaylistResponse
            {
                Id = p.Id,
                PlaylistId = p.PlaylistId,
                Title = p.Title,
                PublishedAt = p.PublishedAt,
                Thumbnail = p.Thumbnail,
                VideoCount = p.VideoCount,
                IsOldChannel = p.IsOldChannel
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<PlaylistResponse>> GetAllByVideoIdAsync(string videoId)
    {
        var playlists = await _context.PlaylistVideos
            .Where(pv => pv.VideoId == videoId)
            .Join(_context.Playlists,
                pv => pv.PlaylistId,
                p => p.PlaylistId,
                (pv, p) => p)
            .Select(p => new PlaylistResponse
            {
                Id = p.Id,
                PlaylistId = p.PlaylistId,
                Title = p.Title,
                PublishedAt = p.PublishedAt,
                Thumbnail = p.Thumbnail,
                VideoCount = p.VideoCount,
                IsOldChannel = p.IsOldChannel
            })
            .ToListAsync();

        return playlists;
    }
}
