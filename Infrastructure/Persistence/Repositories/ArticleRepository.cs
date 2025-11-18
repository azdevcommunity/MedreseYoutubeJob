using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Application.Dtos.Article.Responses;
using YoutubeApiSynchronize.Application.Dtos.Author.Responses;
using YoutubeApiSynchronize.Core.Entities;
using YoutubeApiSynchronize.Core.Interfaces.Article;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly MedreseDbContext _context;

    public ArticleRepository(MedreseDbContext context)
    {
        _context = context;
    }

    public async Task<(List<ArticleProjectionResponse> Items, long TotalCount)> GetAllArticlesAsync(
        int page, int size, List<long>? categoryIds)
    {
        var query = from article in _context.Articles
                    join author in _context.Authors on article.AuthorId equals author.Id
                    select new
                    {
                        Article = article,
                        Author = author
                    };

        if (categoryIds != null && categoryIds.Any())
        {
            query = from item in query
                    join ac in _context.ArticleCategories on item.Article.Id equals ac.ArticleId
                    where categoryIds.Contains(ac.CategoryId)
                    select item;
        }

        var totalCount = await query.Select(x => x.Article.Id).Distinct().LongCountAsync();

        var articles = await query
            .Skip(page * size)
            .Take(size)
            .Select(x => new { x.Article, x.Author })
            .Distinct()
            .ToListAsync();

        var articleIds = articles.Select(x => x.Article.Id).ToList();
        var categoriesDict = await GetCategoriesForArticlesAsync(articleIds);

        var result = articles.Select(x => new ArticleProjectionResponse
        {
            Id = x.Article.Id,
            Title = x.Article.Title,
            Image = x.Article.Image,
            PublishedAt = x.Article.PublishedAt,
            AuthorName = x.Author.Name,
            AuthorImage = x.Author.Image,
            Categories = categoriesDict.ContainsKey(x.Article.Id) ? categoriesDict[x.Article.Id] : new List<string>()
        }).ToList();

        return (result, totalCount);
    }

    public async Task<List<ArticleIdResponse>> GetAllArticleIdsAsync()
    {
        return await _context.Articles
            .Select(a => new ArticleIdResponse { Id = a.Id })
            .ToListAsync();
    }

    public async Task<Core.Entities.Article?> GetByIdAsync(int id)
    {
        return await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ArticleResponse?> GetArticleByIdAsync(int id, bool isAdminRequest)
    {
        var article = await _context.Articles
            .Where(a => a.Id == id)
            .Select(a => new
            {
                Article = a,
                Author = _context.Authors.FirstOrDefault(au => au.Id == a.AuthorId)
            })
            .FirstOrDefaultAsync();

        if (article == null)
            return null;

        var categoryIds = await GetArticleCategoryIdsAsync(id);

        return new ArticleResponse
        {
            Id = article.Article.Id,
            PublishedAt = article.Article.PublishedAt,
            Title = article.Article.Title,
            Content = article.Article.Content,
            Author = article.Author != null ? new AuthorResponse
            {
                Id = article.Author.Id,
                Name = article.Author.Name,
                Image = article.Author.Image
            } : null,
            Categories = categoryIds,
            Image = article.Article.Image,
            ReadCount = article.Article.ReadCount
        };
    }

    public async Task<List<PopularArticleResponse>> GetPopularArticlesAsync()
    {
        return await _context.Articles
            .Join(_context.Authors, a => a.AuthorId, au => au.Id, (a, au) => new { Article = a, Author = au })
            .OrderByDescending(x => x.Article.ReadCount)
            .Take(10)
            .Select(x => new PopularArticleResponse
            {
                Id = x.Article.Id.ToString(),
                Title = x.Article.Title,
                Image = x.Article.Image,
                PublishedAt = x.Article.PublishedAt,
                AuthorName = x.Author.Name,
                AuthorImage = x.Author.Image,
                ReadCount = x.Article.ReadCount
            })
            .ToListAsync();
    }

    public async Task<ArticleStatisticsResponse> GetArticleStatisticsAsync()
    {
        var totalArticles = await _context.Articles.LongCountAsync();
        var totalCategories = await _context.Categories.LongCountAsync();
        var totalAuthors = await _context.Authors.LongCountAsync();
        var totalReadCount = await _context.Articles.SumAsync(a => (long?)a.ReadCount) ?? 0;

        return new ArticleStatisticsResponse
        {
            TotalArticles = totalArticles,
            TotalCategories = totalCategories,
            TotalAuthors = totalAuthors,
            TotalReadCount = totalReadCount
        };
    }

    public async Task IncrementReadCountAsync(int id)
    {
        var article = await GetByIdAsync(id);
        if (article != null)
        {
            article.ReadCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Core.Entities.Article> CreateAsync(Core.Entities.Article article)
    {
        await _context.Articles.AddAsync(article);
        await _context.SaveChangesAsync();
        return article;
    }

    public async Task<Core.Entities.Article> UpdateAsync(Core.Entities.Article article)
    {
        _context.Articles.Update(article);
        await _context.SaveChangesAsync();
        return article;
    }

    public async Task DeleteAsync(int id)
    {
        var article = await GetByIdAsync(id);
        if (article != null)
        {
            await RemoveArticleCategoriesAsync(id);
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteRangeAsync(List<int> ids)
    {
        var articles = await _context.Articles.Where(a => ids.Contains(a.Id)).ToListAsync();
        
        foreach (var id in ids)
        {
            await RemoveArticleCategoriesAsync(id);
        }
        
        _context.Articles.RemoveRange(articles);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByTitleAsync(string title, int? excludeId = null)
    {
        var query = _context.Articles.Where(a => a.Title == title);
        
        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task AddArticleCategoriesAsync(int articleId, HashSet<int> categoryIds)
    {
        var articleCategories = categoryIds.Select(catId => new ArticleCategory
        {
            ArticleId = articleId,
            CategoryId = catId
        }).ToList();

        await _context.ArticleCategories.AddRangeAsync(articleCategories);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveArticleCategoriesAsync(int articleId)
    {
        var articleCategories = await _context.ArticleCategories
            .Where(ac => ac.ArticleId == articleId)
            .ToListAsync();

        _context.ArticleCategories.RemoveRange(articleCategories);
        await _context.SaveChangesAsync();
    }

    public async Task<List<int>> GetArticleCategoryIdsAsync(int articleId)
    {
        return await _context.ArticleCategories
            .Where(ac => ac.ArticleId == articleId)
            .Select(ac => ac.CategoryId)
            .ToListAsync();
    }

    private async Task<Dictionary<int, List<string>>> GetCategoriesForArticlesAsync(List<int> articleIds)
    {
        var categories = await (from ac in _context.ArticleCategories
                               join c in _context.Categories on ac.CategoryId equals c.Id
                               where articleIds.Contains(ac.ArticleId)
                               select new
                               {
                                   ac.ArticleId,
                                   c.Name
                               }).ToListAsync();

        return categories
            .GroupBy(x => x.ArticleId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Name).ToList()
            );
    }
}
