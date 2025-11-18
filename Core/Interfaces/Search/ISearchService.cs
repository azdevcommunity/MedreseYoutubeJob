using YoutubeApiSynchronize.Application.Dtos.Search.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Search;

public interface ISearchService
{
    Task<SearchResponse> SearchAsync(long? categoryId, string? search);
}
