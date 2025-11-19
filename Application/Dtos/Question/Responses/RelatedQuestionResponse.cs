namespace YoutubeApiSynchronize.Application.Dtos.Question.Responses;

public class RelatedQuestionResponse
{
    public int QuestionId { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<QuestionTagResponse> Tags { get; set; } = new();
}
