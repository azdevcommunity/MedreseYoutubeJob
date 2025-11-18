namespace YoutubeApiSynchronize.Application.Dtos.Question.Responses;

public class PagedQuestionResponse
{
    public List<QuestionSearchResponse> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
