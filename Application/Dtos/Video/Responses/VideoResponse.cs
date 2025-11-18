namespace YoutubeApiSynchronize.Application.Dtos.Video.Responses;

public class VideoResponse
{
    public string VideoId { get; set; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; set; }
    public string Thumbnail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? PlaylistId { get; set; }
    public string? Description { get; set; }
}
