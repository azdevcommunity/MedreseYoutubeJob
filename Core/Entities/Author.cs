using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("authors")]
public class Author
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(35)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("image")]
    public string Image { get; set; } = string.Empty;
}
