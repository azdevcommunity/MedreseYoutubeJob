namespace YoutubeApiSynchronize.Application.Dtos.Category.Responses;

public class MenuItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string? Slug { get; set; }
    public List<MenuItemResponse> Children { get; set; } = new();
}
