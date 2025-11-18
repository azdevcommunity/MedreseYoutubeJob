namespace YoutubeApiSynchronize.Core.Interfaces.Category;

public interface ICategoryRepository
{
    Task<List<Entities.Category>> GetAllCategoriesAsync();
    Task<Entities.Category?> GetCategoryByIdAsync(int id);
    Task<Entities.Category> CreateCategoryAsync(Entities.Category category);
    Task<Entities.Category> UpdateCategoryAsync(Entities.Category category);
    Task DeleteCategoryAsync(int id);
    Task<bool> CategoryExistsAsync(int id);
    Task<bool> HasArticlesAsync(int categoryId);
    Task<bool> HasQuestionsAsync(int categoryId);
    Task<bool> HasBooksAsync(int categoryId);
    Task SaveChangesAsync();
}
