namespace YoutubeApiSynchronize.Application.Dtos.Question.Responses;

public class QuestionSearchResponse
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int ViewCount { get; set; }
    public List<QuestionCategoryResponse>? Categories { get; set; }
    public List<QuestionTagResponse>? Tags { get; set; }
}

public class QuestionCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class QuestionTagResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
