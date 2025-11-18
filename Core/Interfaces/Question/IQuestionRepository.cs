using YoutubeApiSynchronize.Application.Dtos.Question.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Question;

public interface IQuestionRepository
{
    Task<(List<QuestionSearchResponse> Items, long TotalCount)> GetAllQuestionsAsync(
        int page, int size, List<int>? tagIds, List<int>? categoryIds, 
        int containsTag, int containsCategory, string? search);
    
    Task<QuestionSearchResponse?> GetQuestionByIdAsync(int id);
    Task<Entities.Question?> GetByIdAsync(int id);
    Task<Entities.Question> CreateAsync(Entities.Question question);
    Task<Entities.Question> UpdateAsync(Entities.Question question);
    Task DeleteAsync(int id);
    Task<bool> ExistsByIdAsync(int id);
    Task<bool> ExistsByQuestionTextAsync(string questionText, int? excludeId = null);
    Task<QuestionStatisticsResponse> GetQuestionStatisticsAsync();
    Task IncrementViewCountAsync(int id);
    Task<List<RelatedQuestionResponse>> GetRelatedQuestionsAsync(int questionId);
    Task AddQuestionCategoriesAsync(int questionId, HashSet<int> categoryIds);
    Task RemoveQuestionCategoriesAsync(int questionId);
    Task AddQuestionTagsAsync(int questionId, HashSet<int> tagIds);
    Task RemoveQuestionTagsAsync(int questionId);
    Task<List<QuestionCategoryResponse>> GetQuestionCategoriesAsync(int questionId);
    Task<List<QuestionTagResponse>> GetQuestionTagsAsync(int questionId);
}
