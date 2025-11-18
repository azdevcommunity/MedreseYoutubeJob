using YoutubeApiSynchronize.Application.Dtos.Video.Requests;
using YoutubeApiSynchronize.Application.Dtos.Video.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Video;

public interface IVideoService
{
    Task<List<VideoResponse>> GetAllAsync();
    Task<(List<VideoResponse> Items, long TotalCount)> GetAllPagingAsync(int page, int size, string? search, int shorts);
    Task<VideoResponse> GetByIdAsync(string videoId);
    Task<List<VideoResponse>> GetByPlaylistIdAsync(string playlistId);
    Task<List<VideoResponse>> GetByPlaylistIdSortedAsync(string playlistId, string sortBy, string sortOrder);
    Task<PagedVideosResponse> GetByPlaylistIdPagedAsync(string playlistId, int page, int size);
    Task<List<VideoResponse>> GetAllSortedAsync(string sortBy, string sortOrder);
    Task<UpdateVideoRequest> UpdateAsync(string videoId, UpdateVideoRequest request);
    Task DeleteAsync(string videoId);
    Task<VideoResponse> GetLatestVideoAsync();
    Task<VideoStatisticsResponse> GetVideoStatisticsAsync();
}
