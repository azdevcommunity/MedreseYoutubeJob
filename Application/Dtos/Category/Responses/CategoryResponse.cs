namespace YoutubeApiSynchronize.Application.Dtos.Category.Responses;

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public bool IsActive { get; set; }
}
