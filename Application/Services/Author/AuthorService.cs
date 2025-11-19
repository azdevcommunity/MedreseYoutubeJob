using Microsoft.Extensions.Configuration;
using YoutubeApiSynchronize.Application.Dtos.Author.Requests;
using YoutubeApiSynchronize.Application.Dtos.Author.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Author;
using YoutubeApiSynchronize.Core.Interfaces.File;

namespace YoutubeApiSynchronize.Application.Services.Author;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;
    private readonly string _folderRoot;

    public AuthorService(
        IAuthorRepository authorRepository, 
        IFileService fileService,
        IConfiguration configuration)
    {
        _authorRepository = authorRepository;
        _fileService = fileService;
        _configuration = configuration;
        _folderRoot = _configuration.GetValue<string>("FolderRoot") ?? "medrese";
    }

    public async Task<List<AuthorResponse>> GetAllAsync()
    {
        var authors = await _authorRepository.GetAllAsync();
        return authors.Select(MapToResponse).ToList();
    }

    public async Task<AuthorResponse> GetByIdAsync(int id)
    {
        var author = await _authorRepository.GetByIdAsync(id);
        if (author == null)
        {
            throw new KeyNotFoundException($"Author with id {id} not found");
        }

        return MapToResponse(author);
    }

    public async Task<AuthorResponse> CreateAsync(CreateAuthorRequest request)
    {
        // Check if author with same name already exists
        var existingAuthors = await _authorRepository.GetAllAsync();
        if (existingAuthors.Any(a => a.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Author with same name already exists");
        }

        // Upload image to Cloudinary
        var imageUrl = await _fileService.UploadFileAsync(request.Image, $"{_folderRoot}/authors");

        var author = new Core.Entities.Author
        {
            Name = request.Name,
            Image = imageUrl
        };

        var createdAuthor = await _authorRepository.CreateAsync(author);
        return MapToResponse(createdAuthor);
    }

    public async Task<AuthorResponse> UpdateAsync(int id, UpdateAuthorRequest request)
    {
        var author = await _authorRepository.GetByIdAsync(id);
        if (author == null)
        {
            throw new KeyNotFoundException($"Author with id {id} not found");
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            author.Name = request.Name;
        }

        // Check if image is base64 and needs to be uploaded
        if (_fileService.IsBase64(request.Image))
        {
            // Delete old image
            await _fileService.DeleteFileAsync(author.Image, $"{_folderRoot}/authors");
            
            // Upload new image
            var imageUrl = await _fileService.UploadFileAsync(request.Image, $"{_folderRoot}/authors");
            author.Image = imageUrl;
        }

        var updatedAuthor = await _authorRepository.UpdateAsync(author);
        return MapToResponse(updatedAuthor);
    }

    public async Task DeleteAsync(int id)
    {
        var author = await _authorRepository.GetByIdAsync(id);
        if (author == null)
        {
            throw new KeyNotFoundException($"Author with id {id} not found");
        }

        // Check if author has associated books
        if (await _authorRepository.HasBooksAsync(id))
        {
            throw new InvalidOperationException("Bu authorun kitabi var sile bilmersen");
        }

        // Check if author has associated articles
        if (await _authorRepository.HasArticlesAsync(id))
        {
            throw new InvalidOperationException("Bu authorun meqalesi var sile bilmersen");
        }

        // Delete author from database
        await _authorRepository.DeleteAsync(id);
        
        // Delete author image from Cloudinary
        await _fileService.DeleteFileAsync(author.Image, $"{_folderRoot}/authors");
    }

    private AuthorResponse MapToResponse(Core.Entities.Author author)
    {
        return new AuthorResponse
        {
            Id = author.Id,
            Name = author.Name,
            Image = author.Image
        };
    }
}
