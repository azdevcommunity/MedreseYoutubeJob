namespace YoutubeApiSynchronize.Application.Dtos.Question.Requests;

public class UpdateQuestionRequest
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string? AnswerType { get; set; }
    public int? AuthorId { get; set; }
    public HashSet<int>? Categories { get; set; }
    public HashSet<int>? Tags { get; set; }
}
