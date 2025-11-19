using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("article_categories")]
public class ArticleCategory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("article_id")]
    public int ArticleId { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }
}
