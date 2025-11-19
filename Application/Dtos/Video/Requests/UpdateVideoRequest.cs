using System.ComponentModel.DataAnnotations;

namespace YoutubeApiSynchronize.Application.Dtos.Video.Requests;

public class UpdateVideoRequest
{
    public string? PublishedAt { get; set; }
    public string? Thumbnail { get; set; }

    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Playlist ID is required")]
    public string PlaylistId { get; set; } = string.Empty;
}
