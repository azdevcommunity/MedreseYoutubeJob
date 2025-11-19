using System.ComponentModel.DataAnnotations;

namespace YoutubeApiSynchronize.Application.Dtos.Category.Requests;

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;
    
    public int? ParentId { get; set; }
}
