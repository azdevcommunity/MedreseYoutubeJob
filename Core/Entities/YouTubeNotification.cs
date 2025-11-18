using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("youtube_notifications")]
public class YouTubeNotification
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("video_id")]
    public string VideoId { get; set; }
    
    [Column("title")]
    public string Title { get; set; }
    
    [Column("published_at")]
    public DateTime PublishedAt { get; set; }
    
    [Column("notification_data")]
    public string? NotificationData { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
