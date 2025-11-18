namespace YoutubeApiSynchronize.Application.Dtos.Video.Responses;

public class PagedVideosResponse
{
    public List<VideoResponse> VideoResponses { get; set; } = new();
    public int ResultForCurrentPage { get; set; }
    public int TotalVideo { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPageCount { get; set; }
}
