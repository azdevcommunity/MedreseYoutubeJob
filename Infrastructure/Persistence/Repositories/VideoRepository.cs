using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Application.Dtos.Common;
using YoutubeApiSynchronize.Application.Dtos.Video.Responses;
using YoutubeApiSynchronize.Core.Entities;
using YoutubeApiSynchronize.Core.Interfaces.Video;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

public class VideoRepository : IVideoRepository
{
    private readonly MedreseDbContext _context;

    public VideoRepository(MedreseDbContext context)
    {
        _context = context;
    }

    public async Task<List<VideoResponse>> GetAllAsync()
    {
        return await _context.Videos
            .OrderByDescending(v => v.PublishedAt)
            .Select(v => new VideoResponse
            {
                VideoId = v.VideoId,
                PublishedAt = v.PublishedAt,
                Thumbnail = v.Thumbnail,
                Title = v.Title,
                PlaylistId = null,
                Description = v.Description
            })
            .ToListAsync();
    }

    public async Task<PagedResponse<VideoResponse>> GetAllPagingAsync(
        int page, int size, string? search, bool isShort, string? playlistId, string? sortBy, string? sortOrder,
        int? maxResult)
    {
        IQueryable<Video> query = _context.Videos.AsQueryable();

        // 🔍 Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v =>
                EF.Functions.ILike(v.Title, $"%{search}%") ||
                (v.Description != null && EF.Functions.ILike(v.Description, $"%{search}%")));
        }

        // 🎬 Shorts filter (only if true)
        if (isShort)
        {
            query = query.Where(v => v.IsShort == true);
        }

        // 📺 Playlist filter
        if (!string.IsNullOrWhiteSpace(playlistId))
        {
            query =
                from v in query
                join pv in _context.PlaylistVideos
                    on v.VideoId equals pv.VideoId
                where pv.PlaylistId == playlistId
                select v;
        }

        // 🎚 Sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            bool desc = (sortOrder ?? "asc").ToLower() == "desc";

            query = desc
                ? query.OrderByDescending(x => EF.Property<object>(x, sortBy))
                : query.OrderBy(x => EF.Property<object>(x, sortBy));
        }
        else
        {
            query = query.OrderByDescending(v => v.PublishedAt);
        }

        // ⏳ Total before paging
        var totalCount = await query.LongCountAsync();

        // 📌 maxResult (limits total returned items)
        if (maxResult.HasValue && maxResult.Value > 0)
            query = query.Take(maxResult.Value);

        // 📄 Paging
        var videos = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(v => new VideoResponse
            {
                VideoId = v.VideoId,
                PublishedAt = v.PublishedAt,
                Thumbnail = v.Thumbnail,
                Title = v.Title,
                PlaylistId = null,
                Description = v.Description
            })
            .ToListAsync();

        return new PagedResponse<VideoResponse>
        {
            Content = videos,
            Page = new PageInfo
            {
                Size = size,
                Number = page,
                TotalElements = (int)totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)size)
            }
        };
    }


    public async Task<VideoResponse?> GetByIdAsync(string videoId)
    {
        return await _context.Videos
            .Where(v => v.VideoId == videoId)
            .Select(v => new VideoResponse
            {
                VideoId = v.VideoId,
                PublishedAt = v.PublishedAt,
                Thumbnail = v.Thumbnail,
                Title = v.Title,
                PlaylistId = null,
                Description = v.Description
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<VideoResponse>> GetByPlaylistIdAsync(string playlistId)
    {
        var playlistVideos = await _context.PlaylistVideos
            .Where(pv => pv.PlaylistId == playlistId)
            .Join(_context.Videos,
                pv => pv.VideoId,
                v => v.VideoId,
                (pv, v) => v)
            .OrderByDescending(v => v.PublishedAt)
            .Select(v => new VideoResponse
            {
                VideoId = v.VideoId,
                PublishedAt = v.PublishedAt,
                Thumbnail = v.Thumbnail,
                Title = v.Title,
                PlaylistId = playlistId,
                Description = v.Description
            })
            .ToListAsync();

        return playlistVideos;
    }

    public async Task<List<VideoResponse>> GetByPlaylistIdSortedAsync(
        string playlistId, string sortBy, string sortOrder)
    {
        var query = _context.PlaylistVideos
            .Where(pv => pv.PlaylistId == playlistId)
            .Join(_context.Videos,
                pv => pv.VideoId,
                v => v.VideoId,
                (pv, v) => v);

        // Apply sorting
        query = sortOrder.ToLower() == "desc"
            ? query.OrderByDescending(v => EF.Property<object>(v, sortBy))
            : query.OrderBy(v => EF.Property<object>(v, sortBy));

        return await query
            .Select(v => new VideoResponse
            {
                VideoId = v.VideoId,
                PublishedAt = v.PublishedAt,
                Thumbnail = v.Thumbnail,
                Title = v.Title,
                PlaylistId = playlistId,
                Description = v.Description
            })
            .ToListAsync();
    }

    public async Task<PagedVideosResponse> GetByPlaylistIdPagedAsync(
        string playlistId, int page, int size)
    {
        var totalVideos = await _context.PlaylistVideos
            .Where(pv => pv.PlaylistId == playlistId)
            .CountAsync();

        if (totalVideos == 0)
        {
            throw new KeyNotFoundException($"Videos not found for playlist id: {playlistId}");
        }

        var totalPageCount = (int)Math.Ceiling(totalVideos / (double)size);

        if (page > totalPageCount)
        {
            throw new ArgumentException($"Invalid number of pages. Maximum number of pages is {totalPageCount}");
        }

        var videos = await _context.PlaylistVideos
            .Where(pv => pv.PlaylistId == playlistId)
            .Join(_context.Videos,
                pv => pv.VideoId,
                v => v.VideoId,
                (pv, v) => v)
            .OrderByDescending(v => v.PublishedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(v => new VideoResponse
            {
                VideoId = v.VideoId,
                PublishedAt = v.PublishedAt,
                Thumbnail = v.Thumbnail,
                Title = v.Title,
                PlaylistId = playlistId,
                Description = v.Description
            })
            .ToListAsync();

        return new PagedVideosResponse
        {
            Content = videos,
            Page = new Application.Dtos.Common.PageInfo
            {
                Size = size,
                Number = page - 1,
                TotalElements = totalVideos,
                TotalPages = totalPageCount
            }
        };
    }

    public async Task<List<VideoResponse>> GetAllSortedAsync(string sortBy, string sortOrder)
    {
        var query = _context.Videos.AsQueryable();

        // Apply sorting
        query = sortOrder.ToLower() == "desc"
            ? query.OrderByDescending(v => EF.Property<object>(v, sortBy))
            : query.OrderBy(v => EF.Property<object>(v, sortBy));

        return await query
            .Select(v => new VideoResponse
            {
                VideoId = v.VideoId,
                PublishedAt = v.PublishedAt,
                Thumbnail = v.Thumbnail,
                Title = v.Title,
                PlaylistId = null,
                Description = v.Description
            })
            .ToListAsync();
    }

    public async Task<Video?> GetVideoEntityByIdAsync(string videoId)
    {
        return await _context.Videos.FirstOrDefaultAsync(v => v.VideoId == videoId);
    }

    public async Task<Video> UpdateAsync(Video video)
    {
        _context.Videos.Update(video);
        await _context.SaveChangesAsync();
        return video;
    }

    public async Task DeleteAsync(string videoId)
    {
        var video = await GetVideoEntityByIdAsync(videoId);
        if (video != null)
        {
            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByVideoIdAsync(string videoId)
    {
        return await _context.Videos.AnyAsync(v => v.VideoId == videoId);
    }

    public async Task<VideoResponse?> GetLatestVideoAsync()
    {
        return await _context.Videos
            .OrderByDescending(v => v.PublishedAt)
            .Select(v => new VideoResponse
            {
                VideoId = v.VideoId,
                PublishedAt = v.PublishedAt,
                Thumbnail = v.Thumbnail,
                Title = v.Title,
                PlaylistId = null,
                Description = v.Description
            })
            .FirstOrDefaultAsync();
    }

    public async Task<VideoStatisticsResponse> GetVideoStatisticsAsync()
    {
        var videoCount = await _context.Videos.LongCountAsync();
        var playlistCount = await _context.Playlists.LongCountAsync();

        var latestChannelStat = await _context.ChannelStats
            .OrderByDescending(cs => cs.Id)
            .FirstOrDefaultAsync();

        return new VideoStatisticsResponse
        {
            VideoCount = videoCount,
            ViewCount = (long)(latestChannelStat?.ViewCount ?? 0),
            PlaylistCount = playlistCount,
            SubscriberCount = (long)(latestChannelStat?.SubscriberCount ?? 0)
        };
    }
}