using YoutubeApiSynchronize.Application.Dtos.Search.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Search;

namespace YoutubeApiSynchronize.Application.Services.Search;

public class SearchService : ISearchService
{
    private readonly ISearchRepository _searchRepository;

    public SearchService(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    public async Task<SearchResponse> SearchAsync(long? categoryId, string? search)
    {
        var categoryIds = categoryId.HasValue ? new List<long> { categoryId.Value } : null;

        var articles = await _searchRepository.SearchArticlesAsync(4, categoryIds, search);
        var videos = await _searchRepository.SearchVideosAsync(4, search);

        return new SearchResponse
        {
            Articles = articles,
            Videos = videos
        };
    }
}
