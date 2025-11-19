namespace YoutubeApiSynchronize.Application.Dtos.Tag.Responses;

public class TagResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
