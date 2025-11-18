using YoutubeApiSynchronize.Application.Dtos.Category.Requests;
using YoutubeApiSynchronize.Application.Dtos.Category.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Category;

namespace YoutubeApiSynchronize.Application.Services.Category;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<Core.Entities.Category>> GetAllCategoriesAsync()
    {
        return await _categoryRepository.GetAllCategoriesAsync();
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(id);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with id {id} not found");
        }

        return MapToResponse(category);
    }

    public async Task<List<MenuItemResponse>> GetCategoryTreeAsync()
    {
        var categories = await _categoryRepository.GetAllCategoriesAsync();
        return BuildCategoryTree(categories, null);
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
    {
        // Check if category with same name already exists
        var existingCategories = await _categoryRepository.GetAllCategoriesAsync();
        if (existingCategories.Any(c => c.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Same category name exists");
        }

        // Validate parent category exists if parentId is provided
        if (request.ParentId.HasValue)
        {
            var parentExists = await _categoryRepository.CategoryExistsAsync(request.ParentId.Value);
            if (!parentExists)
            {
                throw new InvalidOperationException("bu parent id tapilamdi");
            }
        }

        var category = new Core.Entities.Category
        {
            Name = request.Name,
            ParentId = request.ParentId,
            Slug = request.Slug
        };

        var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
        return MapToResponse(createdCategory);
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(id);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with id {id} not found");
        }

        category.Name = request.Name;
        category.ParentId = request.ParentId;
        category.Slug = request.Slug;

        var updatedCategory = await _categoryRepository.UpdateCategoryAsync(category);
        return MapToResponse(updatedCategory);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var exists = await _categoryRepository.CategoryExistsAsync(id);
        if (!exists)
        {
            throw new KeyNotFoundException($"Category with id {id} not found");
        }

        // Check if category is used by articles
        if (await _categoryRepository.HasArticlesAsync(id))
        {
            throw new InvalidOperationException("category use for article doesnt delete");
        }

        // Check if category is used by questions
        if (await _categoryRepository.HasQuestionsAsync(id))
        {
            throw new InvalidOperationException("category use for question doesnt delete");
        }

        // Check if category is used by books
        if (await _categoryRepository.HasBooksAsync(id))
        {
            throw new InvalidOperationException("category use for book doesnt delete");
        }

        await _categoryRepository.DeleteCategoryAsync(id);
    }

    private CategoryResponse MapToResponse(Core.Entities.Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ParentId = category.ParentId,
            Slug = category.Slug
        };
    }

    private List<MenuItemResponse> BuildCategoryTree(List<Core.Entities.Category> categories, int? parentId)
    {
        return categories
            .Where(c => c.ParentId == parentId)
            .Select(c => new MenuItemResponse
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId,
                Slug = c.Slug,
                Children = BuildCategoryTree(categories, c.Id)
            })
            .ToList();
    }
}
