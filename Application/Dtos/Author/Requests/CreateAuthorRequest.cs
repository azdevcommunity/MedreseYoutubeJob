using System.ComponentModel.DataAnnotations;

namespace YoutubeApiSynchronize.Application.Dtos.Author.Requests;

public class CreateAuthorRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(35, ErrorMessage = "Name should be less than 35 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Image is required")]
    public string Image { get; set; } = string.Empty;
}
