using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("question_categories")]
public class QuestionCategory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }
}
