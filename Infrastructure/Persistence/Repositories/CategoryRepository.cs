using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Core.Entities;
using YoutubeApiSynchronize.Core.Interfaces.Category;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly MedreseDbContext _context;

    public CategoryRepository(MedreseDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        _context.Categories.Update(category);
        await SaveChangesAsync();
        return category;
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await GetCategoryByIdAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await SaveChangesAsync();
        }
    }

    public async Task<bool> CategoryExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> HasArticlesAsync(int categoryId)
    {
        // TODO: Implement when ArticleCategory junction table is added
        // return await _context.ArticleCategories.AnyAsync(ac => ac.CategoryId == categoryId);
        return await Task.FromResult(false);
    }

    public async Task<bool> HasQuestionsAsync(int categoryId)
    {
        // TODO: Implement when QuestionCategory junction table is added
        // return await _context.QuestionCategories.AnyAsync(qc => qc.CategoryId == categoryId);
        return await Task.FromResult(false);
    }

    public async Task<bool> HasBooksAsync(int categoryId)
    {
        // TODO: Implement when BookCategory junction table is added
        // return await _context.BookCategories.AnyAsync(bc => bc.CategoryId == categoryId);
        return await Task.FromResult(false);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
