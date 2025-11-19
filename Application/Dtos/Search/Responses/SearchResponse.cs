using YoutubeApiSynchronize.Application.Dtos.Article.Responses;

namespace YoutubeApiSynchronize.Application.Dtos.Search.Responses;

public class SearchResponse
{
    public List<ArticleProjectionResponse> Articles { get; set; } = new();
    public List<VideoSearchResponse> Videos { get; set; } = new();
}

public class VideoSearchResponse
{
    public string VideoId { get; set; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; set; }
    public string Thumbnail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? PlaylistId { get; set; }
    public string? Description { get; set; }
}
