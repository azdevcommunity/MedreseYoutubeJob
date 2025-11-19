using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("books")]
public class Book
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("image")]
    public string? Image { get; set; }
}
