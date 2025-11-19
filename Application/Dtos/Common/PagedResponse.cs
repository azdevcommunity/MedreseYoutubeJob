namespace YoutubeApiSynchronize.Application.Dtos.Common;

public class PagedResponse<T>
{
    public List<T> Content { get; set; } = new();
    public PageInfo Page { get; set; } = new();
}
