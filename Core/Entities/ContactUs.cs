using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("contact_us")]
public class ContactUs
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("subject")]
    public string? Subject { get; set; }

    [Required]
    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("read")]
    public bool Read { get; set; } = false;
}
