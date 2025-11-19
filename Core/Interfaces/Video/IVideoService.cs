
using YoutubeApiSynchronize.Application.Dtos.Common;
using YoutubeApiSynchronize.Application.Dtos.Video.Requests;
using YoutubeApiSynchronize.Application.Dtos.Video.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Video;

public interface IVideoService
{
    Task<List<VideoResponse>> GetAllAsync();
    Task<PagedResponse<VideoResponse>> GetAllPagingAsync(int? page, int? size, string? search, int shorts);
    Task<VideoResponse> GetByIdAsync(string videoId);
    Task<UpdateVideoRequest> UpdateAsync(string videoId, UpdateVideoRequest request);
    Task DeleteAsync(string videoId);
    Task<VideoResponse> GetLatestVideoAsync();
    Task<VideoStatisticsResponse> GetVideoStatisticsAsync();
}
