using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSyncronize.Entity;


[Table("playlist_videos")]
public class PlaylistVideo
{
    [Key]
    public int Id { get; set; }
    
    [Column("video_id")]
    public string VideoId { get; set; } 
    
    [Column("playlist_id")]
    public string PlaylistId { get; set; } 
}