using YoutubeApiSynchronize.Application.Dtos.Question.Requests;
using YoutubeApiSynchronize.Application.Dtos.Question.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Question;

public interface IQuestionService
{
    Task<PagedQuestionResponse> GetAllQuestionsAsync(
        int page, int size, List<int>? tagIds, List<int>? categoryIds,
        int containsTag, int containsCategory, string? search);
    
    Task<QuestionSearchResponse> GetQuestionByIdAsync(int id);
    Task<QuestionResponse> CreateQuestionAsync(CreateQuestionRequest request);
    Task<Entities.Question> UpdateQuestionAsync(int id, UpdateQuestionRequest request);
    Task DeleteQuestionAsync(int id);
    Task<QuestionStatisticsResponse> GetQuestionStatisticsAsync();
    Task IncrementQuestionViewCountAsync(int id);
    Task<List<RelatedQuestionResponse>> GetRelatedQuestionsAsync(int id);
}
