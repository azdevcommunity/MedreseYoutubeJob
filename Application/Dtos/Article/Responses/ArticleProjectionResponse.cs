namespace YoutubeApiSynchronize.Application.Dtos.Article.Responses;

public class ArticleProjectionResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? PublishedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorImage { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
}
