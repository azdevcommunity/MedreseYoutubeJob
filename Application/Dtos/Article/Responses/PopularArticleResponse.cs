namespace YoutubeApiSynchronize.Application.Dtos.Article.Responses;

public class PopularArticleResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? PublishedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorImage { get; set; } = string.Empty;
    public long ReadCount { get; set; }
}
