using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Application.Dtos.Article.Responses;
using YoutubeApiSynchronize.Application.Dtos.Search.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Search;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

public class SearchRepository : ISearchRepository
{
    private readonly MedreseDbContext _context;

    public SearchRepository(MedreseDbContext context)
    {
        _context = context;
    }

    public async Task<List<ArticleProjectionResponse>> SearchArticlesAsync(
        int limit, List<long>? categoryIds, string? search)
    {
        var query = from article in _context.Articles
            join author in _context.Authors on article.AuthorId equals author.Id
            select new
            {
                Article = article,
                Author = author
            };

        // Apply category filter
        if (categoryIds != null && categoryIds.Any())
        {
            query = from item in query
                join ac in _context.ArticleCategories on item.Article.Id equals ac.ArticleId
                where categoryIds.Contains(ac.CategoryId)
                select item;
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Article.Title.Contains(search) ||
                x.Article.Content.Contains(search));
        }

        var articles = await query
            .Take(limit)
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
            Categories = categoriesDict.ContainsKey(x.Article.Id)
                ? categoriesDict[x.Article.Id]
                : new List<string>()
        }).ToList();

        return result;
    }

    public async Task<List<VideoSearchResponse>> SearchVideosAsync(int limit, string? search)
    {
        var query = _context.Videos.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v =>
                EF.Functions.ILike(v.Title, $"%{search}%") ||
                EF.Functions.ILike(v.Description!, $"%{search}%")
            );
        }

        var videos = await query
            .OrderByDescending(v => v.PublishedAt)
            .Take(limit)
            .Select(v => new VideoSearchResponse
            {
                VideoId = v.VideoId,
                PublishedAt = v.PublishedAt,
                Thumbnail = v.Thumbnail,
                Title = v.Title,
                PlaylistId = null, // PlaylistId not directly available in Video entity
                Description = v.Description
            })
            .ToListAsync();

        return videos;
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