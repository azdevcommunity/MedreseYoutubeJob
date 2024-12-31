using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSyncronize.Entity;

[Table("videos")]
public class Video
{
    [Key]
    public int Id { get; set; }
    
    [Column("video_id")]
    public string VideoId { get; set; } 
    
    [Column("title")]
    public string Title { get; set; }
    
    
    [Column("published_at")]
    public string PublishedAt { get; set; } 
    
    [Column("thumbnail")]
    public string Thumbnail { get; set; } 
    
}