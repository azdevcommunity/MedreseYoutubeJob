using YoutubeApiSynchronize.Application.Dtos.Common;
using YoutubeApiSynchronize.Application.Dtos.Video.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Video;

public interface IVideoRepository
{
    Task<List<VideoResponse>> GetAllAsync();
    Task<PagedResponse<VideoResponse>> GetAllPagingAsync(int page, int size, string? search, bool isShort);
    Task<VideoResponse?> GetByIdAsync(string videoId);
    Task<List<VideoResponse>> GetByPlaylistIdAsync(string playlistId);
    Task<List<VideoResponse>> GetByPlaylistIdSortedAsync(string playlistId, string sortBy, string sortOrder);
    Task<PagedVideosResponse> GetByPlaylistIdPagedAsync(string playlistId, int page, int size);
    Task<List<VideoResponse>> GetAllSortedAsync(string sortBy, string sortOrder);
    Task<Entities.Video?> GetVideoEntityByIdAsync(string videoId);
    Task<Entities.Video> UpdateAsync(Entities.Video video);
    Task DeleteAsync(string videoId);
    Task<bool> ExistsByVideoIdAsync(string videoId);
    Task<VideoResponse?> GetLatestVideoAsync();
    Task<VideoStatisticsResponse> GetVideoStatisticsAsync();
}
