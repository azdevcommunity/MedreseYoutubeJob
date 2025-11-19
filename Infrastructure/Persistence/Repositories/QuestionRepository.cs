using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Application.Dtos.Question.Responses;
using YoutubeApiSynchronize.Core.Entities;
using YoutubeApiSynchronize.Core.Interfaces.Question;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly MedreseDbContext _context;

    public QuestionRepository(MedreseDbContext context)
    {
        _context = context;
    }

    public async Task<(List<QuestionSearchResponse> Items, long TotalCount)> GetAllQuestionsAsync(
        int page, int size, List<int>? tagIds, List<int>? categoryIds,
        int containsTag, int containsCategory, string? search)
    {
        var query = _context.Questions.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(q => 
                q.QuestionText.Contains(search) || 
                q.Answer.Contains(search));
        }

        // Apply tag filter
        if (tagIds != null && tagIds.Any())
        {
            var questionIdsWithTags = _context.QuestionTags
                .Where(qt => tagIds.Contains(qt.TagId))
                .Select(qt => qt.QuestionId)
                .Distinct();
            
            query = query.Where(q => questionIdsWithTags.Contains(q.Id));
        }

        // Apply category filter
        if (categoryIds != null && categoryIds.Any())
        {
            var questionIdsWithCategories = _context.QuestionCategories
                .Where(qc => categoryIds.Contains(qc.CategoryId))
                .Select(qc => qc.QuestionId)
                .Distinct();
            
            query = query.Where(q => questionIdsWithCategories.Contains(q.Id));
        }

        var totalCount = await query.LongCountAsync();

        var questions = await query
            .OrderByDescending(q => q.CreatedDate)
            .Skip(page * size)
            .Take(size)
            .Select(q => new QuestionSearchResponse
            {
                Id = q.Id,
                Question = q.QuestionText,
                Answer = q.Answer,
                CreatedDate = q.CreatedDate,
                ViewCount = q.ViewCount
            })
            .ToListAsync();

        // Load categories and tags if requested
        if (containsTag == 1 || containsCategory == 1)
        {
            var questionIds = questions.Select(q => q.Id).ToList();

            if (containsCategory == 1)
            {
                var categoriesDict = await GetCategoriesForQuestionsAsync(questionIds);
                foreach (var question in questions)
                {
                    question.Categories = categoriesDict.ContainsKey(question.Id) 
                        ? categoriesDict[question.Id] 
                        : new List<QuestionCategoryResponse>();
                }
            }

            if (containsTag == 1)
            {
                var tagsDict = await GetTagsForQuestionsAsync(questionIds);
                foreach (var question in questions)
                {
                    question.Tags = tagsDict.ContainsKey(question.Id) 
                        ? tagsDict[question.Id] 
                        : new List<QuestionTagResponse>();
                }
            }
        }

        return (questions, totalCount);
    }

    public async Task<QuestionSearchResponse?> GetQuestionByIdAsync(int id)
    {
        var question = await _context.Questions
            .Where(q => q.Id == id)
            .Select(q => new QuestionSearchResponse
            {
                Id = q.Id,
                Question = q.QuestionText,
                Answer = q.Answer,
                CreatedDate = q.CreatedDate,
                ViewCount = q.ViewCount
            })
            .FirstOrDefaultAsync();

        if (question == null)
            return null;

        question.Categories = await GetQuestionCategoriesAsync(id);
        question.Tags = await GetQuestionTagsAsync(id);

        return question;
    }

    public async Task<Core.Entities.Question?> GetByIdAsync(int id)
    {
        return await _context.Questions.FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Core.Entities.Question> CreateAsync(Core.Entities.Question question)
    {
        await _context.Questions.AddAsync(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task<Core.Entities.Question> CreateQuestionWithRelationsAsync(
        string questionText, string answer, string answerType, int? authorId,
        HashSet<int> categoryIds, HashSet<int> tagIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var question = new Core.Entities.Question
            {
                QuestionText = questionText,
                Answer = answer,
                AnswerType = answerType,
                AuthorId = authorId,
                CreatedDate = DateTime.UtcNow,
                ViewCount = 0
            };

            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();

            // Add question categories if any
            if (categoryIds.Any())
            {
                // Remove any existing categories for this question (in case of orphaned data)
                var existingCategories = await _context.QuestionCategories
                    .Where(qc => qc.QuestionId == question.Id)
                    .ToListAsync();
                
                if (existingCategories.Any())
                {
                    _context.QuestionCategories.RemoveRange(existingCategories);
                    await _context.SaveChangesAsync();
                }

                var questionCategories = categoryIds.Select(catId => new QuestionCategory
                {
                    QuestionId = question.Id,
                    CategoryId = catId
                }).ToList();

                await _context.QuestionCategories.AddRangeAsync(questionCategories);
                await _context.SaveChangesAsync();
            }

            // Add question tags if any
            if (tagIds.Any())
            {
                // Remove any existing tags for this question (in case of orphaned data)
                var existingTags = await _context.QuestionTags
                    .Where(qt => qt.QuestionId == question.Id)
                    .ToListAsync();
                
                if (existingTags.Any())
                {
                    _context.QuestionTags.RemoveRange(existingTags);
                    await _context.SaveChangesAsync();
                }

                var questionTags = tagIds.Select(tagId => new QuestionTag
                {
                    QuestionId = question.Id,
                    TagId = tagId
                }).ToList();

                await _context.QuestionTags.AddRangeAsync(questionTags);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return question;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Core.Entities.Question> UpdateAsync(Core.Entities.Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task DeleteAsync(int id)
    {
        var question = await GetByIdAsync(id);
        if (question != null)
        {
            await RemoveQuestionCategoriesAsync(id);
            await RemoveQuestionTagsAsync(id);
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await _context.Questions.AnyAsync(q => q.Id == id);
    }

    public async Task<bool> ExistsByQuestionTextAsync(string questionText, int? excludeId = null)
    {
        var query = _context.Questions.Where(q => q.QuestionText == questionText);
        
        if (excludeId.HasValue)
        {
            query = query.Where(q => q.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<QuestionStatisticsResponse> GetQuestionStatisticsAsync()
    {
        var totalQuestions = await _context.Questions.LongCountAsync();
        
        var totalCategories = await _context.QuestionCategories
            .Select(qc => qc.CategoryId)
            .Distinct()
            .LongCountAsync();
        
        var totalTags = await _context.QuestionTags
            .Select(qt => qt.TagId)
            .Distinct()
            .LongCountAsync();
        
        var totalViewCount = await _context.Questions.SumAsync(q => (long?)q.ViewCount) ?? 0;

        return new QuestionStatisticsResponse
        {
            TotalQuestions = totalQuestions,
            TotalCategories = totalCategories,
            TotalTags = totalTags,
            TotalViewCount = totalViewCount
        };
    }

    public async Task IncrementViewCountAsync(int id)
    {
        var question = await GetByIdAsync(id);
        if (question != null)
        {
            question.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<RelatedQuestionResponse>> GetRelatedQuestionsAsync(int questionId)
    {
        // Get tags and categories for the current question
        var questionTagIds = await _context.QuestionTags
            .Where(qt => qt.QuestionId == questionId)
            .Select(qt => qt.TagId)
            .ToListAsync();

        var questionCategoryIds = await _context.QuestionCategories
            .Where(qc => qc.QuestionId == questionId)
            .Select(qc => qc.CategoryId)
            .ToListAsync();

        // Find related questions by tags and categories
        var relatedQuestionIds = new List<int>();

        if (questionTagIds.Any())
        {
            relatedQuestionIds.AddRange(
                await _context.QuestionTags
                    .Where(qt => questionTagIds.Contains(qt.TagId) && qt.QuestionId != questionId)
                    .Select(qt => qt.QuestionId)
                    .Distinct()
                    .ToListAsync()
            );
        }

        if (questionCategoryIds.Any())
        {
            relatedQuestionIds.AddRange(
                await _context.QuestionCategories
                    .Where(qc => questionCategoryIds.Contains(qc.CategoryId) && qc.QuestionId != questionId)
                    .Select(qc => qc.QuestionId)
                    .Distinct()
                    .ToListAsync()
            );
        }

        var distinctRelatedIds = relatedQuestionIds.Distinct().Take(4).ToList();

        var relatedQuestions = await _context.Questions
            .Where(q => distinctRelatedIds.Contains(q.Id))
            .Select(q => new RelatedQuestionResponse
            {
                QuestionId = q.Id,
                Question = q.QuestionText
            })
            .ToListAsync();

        // Load tags for each related question
        foreach (var relatedQuestion in relatedQuestions)
        {
            relatedQuestion.Tags = await GetQuestionTagsAsync(relatedQuestion.QuestionId);
        }

        return relatedQuestions;
    }

    public async Task AddQuestionCategoriesAsync(int questionId, HashSet<int> categoryIds)
    {
        var questionCategories = categoryIds.Select(catId => new QuestionCategory
        {
            QuestionId = questionId,
            CategoryId = catId
        }).ToList();

        await _context.QuestionCategories.AddRangeAsync(questionCategories);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveQuestionCategoriesAsync(int questionId)
    {
        var questionCategories = await _context.QuestionCategories
            .Where(qc => qc.QuestionId == questionId)
            .ToListAsync();

        _context.QuestionCategories.RemoveRange(questionCategories);
        await _context.SaveChangesAsync();
    }

    public async Task AddQuestionTagsAsync(int questionId, HashSet<int> tagIds)
    {
        var questionTags = tagIds.Select(tagId => new QuestionTag
        {
            QuestionId = questionId,
            TagId = tagId
        }).ToList();

        await _context.QuestionTags.AddRangeAsync(questionTags);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveQuestionTagsAsync(int questionId)
    {
        var questionTags = await _context.QuestionTags
            .Where(qt => qt.QuestionId == questionId)
            .ToListAsync();

        _context.QuestionTags.RemoveRange(questionTags);
        await _context.SaveChangesAsync();
    }

    public async Task<List<QuestionCategoryResponse>> GetQuestionCategoriesAsync(int questionId)
    {
        return await (from qc in _context.QuestionCategories
                     join c in _context.Categories on qc.CategoryId equals c.Id
                     where qc.QuestionId == questionId
                     select new QuestionCategoryResponse
                     {
                         Id = c.Id,
                         Name = c.Name
                     }).ToListAsync();
    }

    public async Task<List<QuestionTagResponse>> GetQuestionTagsAsync(int questionId)
    {
        return await (from qt in _context.QuestionTags
                     join t in _context.Tags on qt.TagId equals t.Id
                     where qt.QuestionId == questionId
                     select new QuestionTagResponse
                     {
                         Id = t.Id,
                         Name = t.Name
                     }).ToListAsync();
    }

    private async Task<Dictionary<int, List<QuestionCategoryResponse>>> GetCategoriesForQuestionsAsync(List<int> questionIds)
    {
        var categories = await (from qc in _context.QuestionCategories
                               join c in _context.Categories on qc.CategoryId equals c.Id
                               where questionIds.Contains(qc.QuestionId)
                               select new
                               {
                                   qc.QuestionId,
                                   Category = new QuestionCategoryResponse
                                   {
                                       Id = c.Id,
                                       Name = c.Name
                                   }
                               }).ToListAsync();

        return categories
            .GroupBy(x => x.QuestionId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Category).ToList()
            );
    }

    private async Task<Dictionary<int, List<QuestionTagResponse>>> GetTagsForQuestionsAsync(List<int> questionIds)
    {
        var tags = await (from qt in _context.QuestionTags
                         join t in _context.Tags on qt.TagId equals t.Id
                         where questionIds.Contains(qt.QuestionId)
                         select new
                         {
                             qt.QuestionId,
                             Tag = new QuestionTagResponse
                             {
                                 Id = t.Id,
                                 Name = t.Name
                             }
                         }).ToListAsync();

        return tags
            .GroupBy(x => x.QuestionId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Tag).ToList()
            );
    }
}
