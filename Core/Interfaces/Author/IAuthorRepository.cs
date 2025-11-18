namespace YoutubeApiSynchronize.Core.Interfaces.Author;

public interface IAuthorRepository
{
    Task<List<Entities.Author>> GetAllAsync();
    Task<Entities.Author?> GetByIdAsync(int id);
    Task<Entities.Author> CreateAsync(Entities.Author author);
    Task<Entities.Author> UpdateAsync(Entities.Author author);
    Task DeleteAsync(int id);
    Task<bool> HasBooksAsync(int authorId);
    Task<bool> HasArticlesAsync(int authorId);
}
