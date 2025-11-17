using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Entity;

[Table("videos")]
public class Video
{
    [Key] [Column("id")] public int Id { get; set; }

    [Column("video_id")] public string VideoId { get; set; }

    [Column("title")] public string Title { get; set; }

    [Column("published_at")] public DateTimeOffset? PublishedAt { get; set; }

    [Column("thumbnail")] public string Thumbnail { get; set; }

    [Column("is_private")] public bool? IsPrivate { get; set; } = false;

    [Column("is_short")] public bool? IsShort { get; set; } = false;

    [Column(name: "description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("is_old_channel")]
    public bool IsOldChannel { get; set; }
}