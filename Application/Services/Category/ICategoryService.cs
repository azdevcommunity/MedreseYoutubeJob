using YoutubeApiSynchronize.Application.Dtos.Category.Requests;
using YoutubeApiSynchronize.Application.Dtos.Category.Responses;

namespace YoutubeApiSynchronize.Application.Services.Category;

public interface ICategoryService
{
    Task<List<Core.Entities.Category>> GetAllCategoriesAsync();
    Task<CategoryResponse> GetCategoryByIdAsync(int id);
    Task<List<MenuItemResponse>> GetCategoryTreeAsync();
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
    Task DeleteCategoryAsync(int id);
}
