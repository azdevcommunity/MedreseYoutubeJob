using YoutubeApiSynchronize.Application.Dtos.Author.Responses;

namespace YoutubeApiSynchronize.Application.Dtos.Article.Responses;

public class ArticleResponse
{
    public int Id { get; set; }
    public string? PublishedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AuthorResponse? Author { get; set; }
    public List<int> Categories { get; set; } = new();
    public string? Image { get; set; }
    public long ReadCount { get; set; }
}
