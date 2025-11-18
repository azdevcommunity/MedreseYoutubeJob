using YoutubeApiSynchronize.Application.Dtos.Article.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Article;

public interface IArticleRepository
{
    Task<(List<ArticleProjectionResponse> Items, long TotalCount)> GetAllArticlesAsync(
        int page, int size, List<long>? categoryIds);
    
    Task<List<ArticleIdResponse>> GetAllArticleIdsAsync();
    
    Task<Core.Entities.Article?> GetByIdAsync(int id);
    
    Task<ArticleResponse?> GetArticleByIdAsync(int id, bool isAdminRequest);
    
    Task<List<PopularArticleResponse>> GetPopularArticlesAsync();
    
    Task<ArticleStatisticsResponse> GetArticleStatisticsAsync();
    
    Task IncrementReadCountAsync(int id);
    
    Task<Core.Entities.Article> CreateAsync(Core.Entities.Article article);
    
    Task<Core.Entities.Article> UpdateAsync(Core.Entities.Article article);
    
    Task DeleteAsync(int id);
    
    Task DeleteRangeAsync(List<int> ids);
    
    Task<bool> ExistsByTitleAsync(string title, int? excludeId = null);
    
    Task AddArticleCategoriesAsync(int articleId, HashSet<int> categoryIds);
    
    Task RemoveArticleCategoriesAsync(int articleId);
    
    Task<List<int>> GetArticleCategoryIdsAsync(int articleId);
}
