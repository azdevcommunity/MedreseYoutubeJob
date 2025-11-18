using YoutubeApiSynchronize.Application.Dtos.Video.Requests;
using YoutubeApiSynchronize.Application.Dtos.Video.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Video;

namespace YoutubeApiSynchronize.Application.Services.Video;

public class VideoService : IVideoService
{
    private readonly IVideoRepository _videoRepository;

    public VideoService(IVideoRepository videoRepository)
    {
        _videoRepository = videoRepository;
    }

    public async Task<List<VideoResponse>> GetAllAsync()
    {
        return await _videoRepository.GetAllAsync();
    }

    public async Task<(List<VideoResponse> Items, long TotalCount)> GetAllPagingAsync(
        int page, int size, string? search, int shorts)
    {
        if (size <= 0)
        {
            throw new ArgumentException("Size must be at least 1");
        }

        if (size > 40)
        {
            throw new ArgumentException("Size cannot be greater than 40");
        }

        bool isShort = shorts == 1;
        return await _videoRepository.GetAllPagingAsync(page, size, search, isShort);
    }

    public async Task<VideoResponse> GetByIdAsync(string videoId)
    {
        var video = await _videoRepository.GetByIdAsync(videoId);
        
        if (video == null)
        {
            throw new KeyNotFoundException($"Video not found with id: {videoId}");
        }

        return video;
    }

    public async Task<List<VideoResponse>> GetByPlaylistIdAsync(string playlistId)
    {
        return await _videoRepository.GetByPlaylistIdAsync(playlistId);
    }

    public async Task<List<VideoResponse>> GetByPlaylistIdSortedAsync(
        string playlistId, string sortBy, string sortOrder)
    {
        return await _videoRepository.GetByPlaylistIdSortedAsync(playlistId, sortBy, sortOrder);
    }

    public async Task<PagedVideosResponse> GetByPlaylistIdPagedAsync(
        string playlistId, int page, int size)
    {
        if (size <= 0)
        {
            throw new ArgumentException("Invalid maxResult value. maxResult cannot be zero or negative");
        }

        return await _videoRepository.GetByPlaylistIdPagedAsync(playlistId, page, size);
    }

    public async Task<List<VideoResponse>> GetAllSortedAsync(string sortBy, string sortOrder)
    {
        return await _videoRepository.GetAllSortedAsync(sortBy, sortOrder);
    }

    public async Task<UpdateVideoRequest> UpdateAsync(string videoId, UpdateVideoRequest request)
    {
        var video = await _videoRepository.GetVideoEntityByIdAsync(videoId);
        
        if (video == null)
        {
            throw new KeyNotFoundException($"Video not found with id: {videoId}");
        }

        video.Title = request.Title;
        
        if (!string.IsNullOrEmpty(request.PublishedAt))
        {
            if (DateTimeOffset.TryParse(request.PublishedAt, out var publishedAt))
            {
                video.PublishedAt = publishedAt;
            }
        }

        if (!string.IsNullOrEmpty(request.Thumbnail))
        {
            video.Thumbnail = request.Thumbnail;
        }

        await _videoRepository.UpdateAsync(video);

        return request;
    }

    public async Task DeleteAsync(string videoId)
    {
        if (!await _videoRepository.ExistsByVideoIdAsync(videoId))
        {
            throw new KeyNotFoundException($"Video not found with id: {videoId}");
        }

        await _videoRepository.DeleteAsync(videoId);
    }

    public async Task<VideoResponse> GetLatestVideoAsync()
    {
        var video = await _videoRepository.GetLatestVideoAsync();
        
        if (video == null)
        {
            throw new KeyNotFoundException("No videos found");
        }

        return video;
    }

    public async Task<VideoStatisticsResponse> GetVideoStatisticsAsync()
    {
        return await _videoRepository.GetVideoStatisticsAsync();
    }
}
