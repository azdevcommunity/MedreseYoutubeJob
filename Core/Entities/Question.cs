using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("questions")]
public class Question
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("question", TypeName = "text")]
    public string QuestionText { get; set; } = string.Empty;

    [Required]
    [Column("answer", TypeName = "text")]
    public string Answer { get; set; } = string.Empty;

    [Required]
    [Column("answer_type")]
    [MaxLength(10)]
    public string AnswerType { get; set; } = string.Empty;

    [Column("author_id")]
    public int? AuthorId { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("view_count")]
    public int ViewCount { get; set; } = 0;
}
