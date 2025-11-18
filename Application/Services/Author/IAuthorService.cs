using YoutubeApiSynchronize.Application.Dtos.Author.Requests;
using YoutubeApiSynchronize.Application.Dtos.Author.Responses;

namespace YoutubeApiSynchronize.Application.Services.Author;

public interface IAuthorService
{
    Task<List<AuthorResponse>> GetAllAsync();
    Task<AuthorResponse> GetByIdAsync(int id);
    Task<AuthorResponse> CreateAsync(CreateAuthorRequest request);
    Task<AuthorResponse> UpdateAsync(int id, UpdateAuthorRequest request);
    Task DeleteAsync(int id);
}
