using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("statistics")]
public class Statistic
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("platform_name")]
    public string? PlatformName { get; set; }

    [Column("view_count")]
    public string? ViewCount { get; set; }

    [Required]
    [Column("subscriber_count")]
    public string SubscriberCount { get; set; } = string.Empty;

    [Column("video_count")]
    public string? VideoCount { get; set; }
}
