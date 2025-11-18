using System.ComponentModel.DataAnnotations;

namespace YoutubeApiSynchronize.Application.Dtos.Tag.Requests;

public class UpdateTagRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(50, ErrorMessage = "Name should be less than 50 characters")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
