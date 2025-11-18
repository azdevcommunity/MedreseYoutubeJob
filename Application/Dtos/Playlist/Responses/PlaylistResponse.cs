namespace YoutubeApiSynchronize.Application.Dtos.Playlist.Responses;

public class PlaylistResponse
{
    public int Id { get; set; }
    public string PlaylistId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; set; }
    public string Thumbnail { get; set; } = string.Empty;
    public long VideoCount { get; set; }
    public bool IsOldChannel { get; set; }
}
