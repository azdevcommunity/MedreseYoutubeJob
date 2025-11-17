using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Entity;

[Table("playlists")]
public class Playlist
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("playlist_id")]
    public string PlaylistId { get; set; } 
    
    [Column("title")]
    public string Title { get; set; } 

    [Column("published_at")]
    public DateTimeOffset? PublishedAt { get; set; }

    [Column("thumbnail")]
    public string Thumbnail { get; set; }

    [Column("video_count")]
    public long VideoCount { get; set; }
    
    [Column("is_old_channel")]
    public bool IsOldChannel { get; set; }
}