using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("playlist_videos")]
public class PlaylistVideo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("video_id")]
    public string VideoId { get; set; } 
    
    [Column("playlist_id")]
    public string PlaylistId { get; set; } 
}
