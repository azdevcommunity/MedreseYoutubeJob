using YoutubeApiSynchronize.Application.Dtos.Tag.Requests;
using YoutubeApiSynchronize.Application.Dtos.Tag.Responses;

namespace YoutubeApiSynchronize.Application.Services.Tag;

public interface ITagService
{
    Task<List<TagResponse>> GetAllAsync();
    Task<TagResponse> GetByIdAsync(int id);
    Task<TagResponse> CreateAsync(CreateTagRequest request);
    Task<TagResponse> UpdateAsync(int id, UpdateTagRequest request);
    Task DeleteAsync(int id);
}
