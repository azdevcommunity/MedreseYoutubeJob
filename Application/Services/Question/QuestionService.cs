using YoutubeApiSynchronize.Application.Dtos.Question.Requests;
using YoutubeApiSynchronize.Application.Dtos.Question.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Question;

namespace YoutubeApiSynchronize.Application.Services.Question;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;

    public QuestionService(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public async Task<PagedQuestionResponse> GetAllQuestionsAsync(
        int page, int size, List<int>? tagIds, List<int>? categoryIds,
        int containsTag, int containsCategory, string? search)
    {
        var (items, totalCount) = await _questionRepository.GetAllQuestionsAsync(
            page, size, tagIds, categoryIds, containsTag, containsCategory, search);
        
        var totalPages = (int)Math.Ceiling(totalCount / (double)size);

        return new PagedQuestionResponse
        {
            Content = items,
            Page = new Application.Dtos.Common.PageInfo
            {
                Size = size,
                Number = page,
                TotalElements = (int)totalCount,
                TotalPages = totalPages
            }
        };
    }

    public async Task<QuestionSearchResponse> GetQuestionByIdAsync(int id)
    {
        var question = await _questionRepository.GetQuestionByIdAsync(id);
        
        if (question == null)
        {
            throw new KeyNotFoundException($"Question with id {id} not found");
        }

        return question;
    }

    public async Task<QuestionResponse> CreateQuestionAsync(CreateQuestionRequest request)
    {
        // Check if question with same text already exists
        if (await _questionRepository.ExistsByQuestionTextAsync(request.Question))
        {
            throw new InvalidOperationException("Question with same text already exists");
        }

        // Use transaction to ensure atomicity
        var createdQuestion = await _questionRepository.CreateQuestionWithRelationsAsync(
            request.Question,
            request.Answer,
            request.AnswerType,
            request.Author ,
            request.Categories,
            request.Tags
        );

        return new QuestionResponse
        {
            Id = createdQuestion.Id,
            Question = createdQuestion.QuestionText,
            Answer = createdQuestion.Answer,
            Categories = request.Categories
        };
    }

    public async Task<Core.Entities.Question> UpdateQuestionAsync(int id, UpdateQuestionRequest request)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
        {
            throw new KeyNotFoundException($"Question with id {id} not found");
        }

        // Check if question text is being changed and if new text already exists
        if (!string.IsNullOrEmpty(request.Question) && request.Question != question.QuestionText)
        {
            if (await _questionRepository.ExistsByQuestionTextAsync(request.Question, id))
            {
                throw new InvalidOperationException("Question with same text already exists");
            }
            question.QuestionText = request.Question;
        }

        if (!string.IsNullOrEmpty(request.Answer))
        {
            question.Answer = request.Answer;
        }

        if (!string.IsNullOrEmpty(request.AnswerType))
        {
            question.AnswerType = request.AnswerType;
        }

        if (request.AuthorId.HasValue)
        {
            question.AuthorId = request.AuthorId.Value;
        }

        // Update categories if provided
        if (request.Categories != null)
        {
            await _questionRepository.RemoveQuestionCategoriesAsync(id);
            if (request.Categories.Any())
            {
                await _questionRepository.AddQuestionCategoriesAsync(id, request.Categories);
            }
        }

        // Update tags if provided
        if (request.Tags != null)
        {
            await _questionRepository.RemoveQuestionTagsAsync(id);
            if (request.Tags.Any())
            {
                await _questionRepository.AddQuestionTagsAsync(id, request.Tags);
            }
        }

        return await _questionRepository.UpdateAsync(question);
    }

    public async Task DeleteQuestionAsync(int id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
        {
            throw new KeyNotFoundException($"Question with id {id} not found");
        }

        await _questionRepository.DeleteAsync(id);
    }

    public async Task<QuestionStatisticsResponse> GetQuestionStatisticsAsync()
    {
        return await _questionRepository.GetQuestionStatisticsAsync();
    }

    public async Task IncrementQuestionViewCountAsync(int id)
    {
        if (!await _questionRepository.ExistsByIdAsync(id))
        {
            throw new KeyNotFoundException($"Question with id {id} not found");
        }

        await _questionRepository.IncrementViewCountAsync(id);
    }

    public async Task<List<RelatedQuestionResponse>> GetRelatedQuestionsAsync(int id)
    {
        if (!await _questionRepository.ExistsByIdAsync(id))
        {
            throw new KeyNotFoundException($"Question with id {id} not found");
        }

        return await _questionRepository.GetRelatedQuestionsAsync(id);
    }
}
