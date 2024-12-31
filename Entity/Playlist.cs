using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSyncronize.Entity;

[Table("playlists")]
public class Playlist
{
    [Key]
    public int Id { get; set; }
    
    [Column("playlist_id")]
    public string PlaylistId { get; set; } 
    
    [Column("title")]
    public string Title { get; set; } 

    [Column("published_at")]
    public string PublishedAt { get; set; }

    [Column("thumbnail")]
    public string Thumbnail { get; set; }

    [Column("video_count")]
    public long VideoCount { get; set; }
}