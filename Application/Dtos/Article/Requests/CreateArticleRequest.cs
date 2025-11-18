using System.ComponentModel.DataAnnotations;

namespace YoutubeApiSynchronize.Application.Dtos.Article.Requests;

public class CreateArticleRequest
{
    [Required(ErrorMessage = "Image is required")]
    public string Image { get; set; } = string.Empty;

    [Required(ErrorMessage = "PublishedAt is required")]
    public string PublishedAt { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author is required")]
    public int AuthorId { get; set; }

    [Required(ErrorMessage = "Categories is required")]
    public HashSet<int> Categories { get; set; } = new();
}
