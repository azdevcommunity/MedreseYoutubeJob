using YoutubeApiSynchronize.Application.Dtos.Common;
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

    public async Task<PagedResponse<VideoResponse>> GetAllPagingAsync(
        int? page, int? size, string? search, int shorts,
        string? playlistId, string? sortBy, string? sortOrder, int? maxResult)
    {
        if (page is <= 0)
        {
            page = 1;
        }

        if (size is > 40 or <= 0)
        {
            size = 10;
        }

        int newPage = page ?? 1;
        int newSize = size ?? 10;
        bool isShort = shorts == 1;

        if (sortBy is null)
        {
            sortBy = "Title";
        }
        
        return await _videoRepository.GetAllPagingAsync(newPage, newSize, search, isShort, playlistId, sortBy,
            sortOrder, maxResult);
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