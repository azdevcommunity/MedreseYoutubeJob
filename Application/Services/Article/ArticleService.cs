using Microsoft.Extensions.Configuration;
using YoutubeApiSynchronize.Application.Dtos.Article.Requests;
using YoutubeApiSynchronize.Application.Dtos.Article.Responses;
using YoutubeApiSynchronize.Application.Dtos.Author.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Article;
using YoutubeApiSynchronize.Core.Interfaces.Author;
using YoutubeApiSynchronize.Core.Interfaces.File;

namespace YoutubeApiSynchronize.Application.Services.Article;

public class ArticleService : IArticleService
{
    private readonly IArticleRepository _articleRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;
    private readonly string _folderRoot;

    public ArticleService(
        IArticleRepository articleRepository,
        IAuthorRepository authorRepository,
        IFileService fileService,
        IConfiguration configuration)
    {
        _articleRepository = articleRepository;
        _authorRepository = authorRepository;
        _fileService = fileService;
        _configuration = configuration;
        _folderRoot = _configuration.GetValue<string>("FolderRoot") ?? "medrese";
    }

    public async Task<PagedArticleResponse> GetAllArticlesAsync(int page, int size, List<long>? categoryIds)
    {
        var (items, totalCount) = await _articleRepository.GetAllArticlesAsync(page, size, categoryIds);
        var totalPages = (int)Math.Ceiling(totalCount / (double)size);

        return new PagedArticleResponse
        {
            Content = items,
            Page = new Application.Dtos.Common.PageInfo
            {
                Size = size,
                Number = page,
                TotalElements = (int)totalCount,
                TotalPages = totalPages
            }
        };
    }

    public async Task<List<ArticleIdResponse>> GetAllArticleIdsAsync()
    {
        return await _articleRepository.GetAllArticleIdsAsync();
    }

    public async Task<ArticleResponse> GetArticleByIdAsync(int id, bool isAdminRequest)
    {
        var article = await _articleRepository.GetArticleByIdAsync(id, isAdminRequest);
        
        if (article == null)
        {
            throw new KeyNotFoundException($"Article with id {id} not found");
        }

        if (!isAdminRequest)
        {
            await _articleRepository.IncrementReadCountAsync(id);
        }

        return article;
    }

    public async Task<List<PopularArticleResponse>> GetPopularArticlesAsync()
    {
        return await _articleRepository.GetPopularArticlesAsync();
    }

    public async Task<ArticleStatisticsResponse> GetArticleStatisticsAsync()
    {
        return await _articleRepository.GetArticleStatisticsAsync();
    }

    public async Task IncrementReadCountAsync(int id)
    {
        await _articleRepository.IncrementReadCountAsync(id);
    }

    public async Task<ArticleResponse> CreateArticleAsync(CreateArticleRequest request)
    {
        // Check if article with same title already exists
        if (await _articleRepository.ExistsByTitleAsync(request.Title))
        {
            throw new InvalidOperationException("Article with same title already exists");
        }

        // Verify author exists
        var author = await _authorRepository.GetByIdAsync(request.AuthorId);
        if (author == null)
        {
            throw new KeyNotFoundException($"Author with id {request.AuthorId} not found");
        }

        // Upload image to Cloudinary first (before transaction)
        var imageUrl = await _fileService.UploadFileAsync(request.Image, $"{_folderRoot}/articles");

        try
        {
            // Use transaction to ensure atomicity
            var createdArticle = await _articleRepository.CreateArticleWithCategoriesAsync(
                request.PublishedAt,
                request.Title,
                request.Content,
                imageUrl,
                request.AuthorId,
                request.Categories
            );

            // Fetch and return complete article response
            var articleResponse = await _articleRepository.GetArticleByIdAsync(createdArticle.Id, true);
            return articleResponse ?? throw new InvalidOperationException("Failed to retrieve created article");
        }
        catch
        {
            // If article creation fails, delete the uploaded image
            await _fileService.DeleteFileAsync(imageUrl, $"{_folderRoot}/articles");
            throw;
        }
    }

    public async Task<Core.Entities.Article> UpdateArticleAsync(int id, UpdateArticleRequest request)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null)
        {
            throw new KeyNotFoundException($"Article with id {id} not found");
        }

        // Check if title is being changed and if new title already exists
        if (!string.IsNullOrEmpty(request.Title) && request.Title != article.Title)
        {
            if (await _articleRepository.ExistsByTitleAsync(request.Title, id))
            {
                throw new InvalidOperationException("Article with same title already exists");
            }
            article.Title = request.Title;
        }

        if (!string.IsNullOrEmpty(request.PublishedAt))
        {
            article.PublishedAt = request.PublishedAt;
        }

        if (!string.IsNullOrEmpty(request.Content))
        {
            article.Content = request.Content;
        }

        if (request.AuthorId.HasValue)
        {
            var author = await _authorRepository.GetByIdAsync(request.AuthorId.Value);
            if (author == null)
            {
                throw new KeyNotFoundException($"Author with id {request.AuthorId} not found");
            }
            article.AuthorId = request.AuthorId.Value;
        }

        // Handle image update
        if (!string.IsNullOrEmpty(request.Image) && _fileService.IsBase64(request.Image))
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(article.Image))
            {
                await _fileService.DeleteFileAsync(article.Image, $"{_folderRoot}/articles");
            }
            
            // Upload new image
            var imageUrl = await _fileService.UploadFileAsync(request.Image, $"{_folderRoot}/articles");
            article.Image = imageUrl;
        }

        // Update categories if provided
        if (request.Categories != null)
        {
            await _articleRepository.RemoveArticleCategoriesAsync(id);
            if (request.Categories.Any())
            {
                await _articleRepository.AddArticleCategoriesAsync(id, request.Categories);
            }
        }

        return await _articleRepository.UpdateAsync(article);
    }

    public async Task DeleteArticleAsync(int id)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null)
        {
            throw new KeyNotFoundException($"Article with id {id} not found");
        }

        // Delete article from database (categories will be deleted via repository)
        await _articleRepository.DeleteAsync(id);

        // Delete article image from Cloudinary if exists
        if (!string.IsNullOrEmpty(article.Image))
        {
            await _fileService.DeleteFileAsync(article.Image, $"{_folderRoot}/articles");
        }
    }

    public async Task DeleteArticlesAsync(DeleteArticlesRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return;
        }

        // Get all articles to delete their images
        var articles = new List<Core.Entities.Article>();
        foreach (var id in request.Ids)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article != null)
            {
                articles.Add(article);
            }
        }

        // Delete articles from database
        await _articleRepository.DeleteRangeAsync(request.Ids);

        // Delete images from Cloudinary
        foreach (var article in articles)
        {
            if (!string.IsNullOrEmpty(article.Image))
            {
                await _fileService.DeleteFileAsync(article.Image, $"{_folderRoot}/articles");
            }
        }
    }
}
