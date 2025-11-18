using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.Dtos.Category.Requests;
using YoutubeApiSynchronize.Application.Dtos.Category.Responses;
using YoutubeApiSynchronize.Application.Services.Category;
using YoutubeApiSynchronize.Core.Entities;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger _logger;

    public CategoryController(ICategoryService categoryService, ILogger logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetAllCategories()
    {
        _logger.Information("GetAllCategories endpoint called");
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById(int id)
    {
        _logger.Information("GetCategoryById endpoint called with id: {Id}", id);
        var category = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(category);
    }

    [HttpGet("menu")]
    public async Task<ActionResult<List<MenuItemResponse>>> GetMenuItems()
    {
        _logger.Information("GetMenuItems endpoint called");
        var menuItems = await _categoryService.GetCategoryTreeAsync();
        return Ok(menuItems);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        _logger.Information("CreateCategory endpoint called");
        var category = await _categoryService.CreateCategoryAsync(request);
        return Ok(category);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        _logger.Information("UpdateCategory endpoint called with id: {Id}", id);
        var category = await _categoryService.UpdateCategoryAsync(id, request);
        return Ok(category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        _logger.Information("DeleteCategory endpoint called with id: {Id}", id);
        await _categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }
}