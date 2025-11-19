using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YoutubeApiSynchronize.Core.Entities;

[Table("categories")]
public class Category
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("parent_id")]
    public int? ParentId { get; set; }
    
    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}
