namespace YoutubeApiSynchronize.Options;

public class ShortPlaylistsOptions
{
    public List<ShortPlaylist> ShortPlaylists { get; set; } = new();
}

public class ShortPlaylist
{
    public string PlaylistId { get; set; }
    public List<string>? ExceptionalVideos { get; set; } = new();
}