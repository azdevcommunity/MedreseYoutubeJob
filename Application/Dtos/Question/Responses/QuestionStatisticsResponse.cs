namespace YoutubeApiSynchronize.Application.Dtos.Question.Responses;

public class QuestionStatisticsResponse
{
    public long TotalQuestions { get; set; }
    public long TotalCategories { get; set; }
    public long TotalTags { get; set; }
    public long TotalViewCount { get; set; }
}
