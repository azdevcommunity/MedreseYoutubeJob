using System.ComponentModel.DataAnnotations;

namespace YoutubeApiSynchronize.Application.Dtos.Question.Requests;

public class CreateQuestionRequest
{
    [Required(ErrorMessage = "Question is required")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "Answer is required")]
    public string Answer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Answer type is required")]
    public string AnswerType { get; set; } = string.Empty;

    public int? Author { get; set; }

    public HashSet<int> Categories { get; set; } = new();

    public HashSet<int> Tags { get; set; } = new();
}
