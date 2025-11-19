using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("articles")]
public class Article
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("published_at")]
    public string? PublishedAt { get; set; }

    [Required]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("content", TypeName = "text")]
    public string Content { get; set; } = string.Empty;

    [Column("image")]
    public string? Image { get; set; }

    [Column("read_count")]
    public long ReadCount { get; set; } = 0;

    [Column("author_id")]
    public int AuthorId { get; set; }
}
