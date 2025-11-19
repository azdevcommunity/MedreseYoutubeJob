using YoutubeApiSynchronize.Application.Dtos.Article.Requests;
using YoutubeApiSynchronize.Application.Dtos.Article.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Article;

public interface IArticleService
{
    Task<PagedArticleResponse> GetAllArticlesAsync(int page, int size, List<long>? categoryIds);
    Task<List<ArticleIdResponse>> GetAllArticleIdsAsync();
    Task<ArticleResponse> GetArticleByIdAsync(int id, bool isAdminRequest);
    Task<List<PopularArticleResponse>> GetPopularArticlesAsync();
    Task<ArticleStatisticsResponse> GetArticleStatisticsAsync();
    Task IncrementReadCountAsync(int id);
    Task<ArticleResponse> CreateArticleAsync(CreateArticleRequest request);
    Task<Core.Entities.Article> UpdateArticleAsync(int id, UpdateArticleRequest request);
    Task DeleteArticleAsync(int id);
    Task DeleteArticlesAsync(DeleteArticlesRequest request);
}
