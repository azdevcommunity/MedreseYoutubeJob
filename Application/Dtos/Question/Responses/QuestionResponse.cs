namespace YoutubeApiSynchronize.Application.Dtos.Question.Responses;

public class QuestionResponse
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public HashSet<int> Categories { get; set; } = new();
}
