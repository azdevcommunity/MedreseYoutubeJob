using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Entity;

public class YouTubeNotification
{
    [Column("id")]
    public int Id { get; set; } // Primary Key, otomatik artan
    [Column("video_id")]
    public string VideoId { get; set; } // Video ID'si
    [Column("title")]
    public string Title { get; set; } // Video Başlığı
    [Column("published_at")]
    public DateTime PublishedAt { get; set; } // Yayınlanma Zamanı
    [Column("notification_data")]
    public string? NotificationData { get; set; } // JSON formatındaki Bildirim Verisi
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } // Kaydın Oluşturulma Zamanı
}