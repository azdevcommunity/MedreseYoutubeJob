namespace YoutubeApiSynchronize.Application.Dtos.Article.Requests;

public class UpdateArticleRequest
{
    public string? PublishedAt { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int? AuthorId { get; set; }
    public HashSet<int>? Categories { get; set; }
    public string? Image { get; set; }
}
