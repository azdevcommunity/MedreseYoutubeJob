using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Entity;

[Table(("channel_stat"))]
public class ChannelStat
{
    [Column("id")] public int Id { get; set; }
    [Column("view_count")] public ulong? ViewCount { get; set; }
    [Column("subscriber_count")] public ulong? SubscriberCount { get; set; }
    [Column("video_count")] public ulong? VideoCount { get; set; }
    [Column("hidden_subscriber_count")] public bool? HiddenSubscriberCount { get; set; }
    [Column("captured_at_utc")] public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
}