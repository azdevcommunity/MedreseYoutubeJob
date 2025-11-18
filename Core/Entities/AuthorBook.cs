using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("author_books")]
public class AuthorBook
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("author_id")]
    public int AuthorId { get; set; }

    [Column("book_id")]
    public int BookId { get; set; }
}
