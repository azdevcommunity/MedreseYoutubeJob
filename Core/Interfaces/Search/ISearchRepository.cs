using YoutubeApiSynchronize.Application.Dtos.Article.Responses;
using YoutubeApiSynchronize.Application.Dtos.Search.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Search;

public interface ISearchRepository
{
    Task<List<ArticleProjectionResponse>> SearchArticlesAsync(int limit, List<long>? categoryIds, string? search);
    Task<List<VideoSearchResponse>> SearchVideosAsync(int limit, string? search);
}
