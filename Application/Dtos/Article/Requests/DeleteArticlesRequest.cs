namespace YoutubeApiSynchronize.Application.Dtos.Article.Requests;

public class DeleteArticlesRequest
{
    public List<int> Ids { get; set; } = new();
}
