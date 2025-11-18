using YoutubeApiSynchronize.Application.Dtos.Statistic.Requests;
using YoutubeApiSynchronize.Application.Dtos.Statistic.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Statistic;

namespace YoutubeApiSynchronize.Application.Services.Statistic;

public class StatisticService : IStatisticService
{
    private readonly IStatisticRepository _statisticRepository;

    public StatisticService(IStatisticRepository statisticRepository)
    {
        _statisticRepository = statisticRepository;
    }

    public async Task<List<Core.Entities.Statistic>> GetAllStatisticsAsync()
    {
        return await _statisticRepository.GetAllAsync();
    }

    public async Task<Core.Entities.Statistic> GetStatisticByIdAsync(int id)
    {
        var statistic = await _statisticRepository.GetByIdAsync(id);
        
        if (statistic == null)
        {
            throw new KeyNotFoundException($"Statistic with id {id} not found");
        }

        return statistic;
    }

    public async Task<StatisticResponse> CreateStatisticAsync(CreateStatisticRequest request)
    {
        var statistic = new Core.Entities.Statistic
        {
            ViewCount = request.ViewCount,
            SubscriberCount = request.SubscriberCount,
            VideoCount = request.VideoCount,
            PlatformName = null // Set based on business logic if needed
        };

        var createdStatistic = await _statisticRepository.CreateAsync(statistic);

        return new StatisticResponse
        {
            Id = createdStatistic.Id,
            ViewCount = createdStatistic.ViewCount,
            SubscriberCount = createdStatistic.SubscriberCount,
            VideoCount = createdStatistic.VideoCount,
            PlatformName = createdStatistic.PlatformName
        };
    }

    public async Task<Core.Entities.Statistic> UpdateStatisticAsync(int id, UpdateStatisticRequest request)
    {
        var statistic = await _statisticRepository.GetByIdAsync(id);
        
        if (statistic == null)
        {
            throw new KeyNotFoundException($"Statistic with id {id} not found");
        }

        if (!string.IsNullOrEmpty(request.PlatformName))
        {
            statistic.PlatformName = request.PlatformName;
        }

        if (!string.IsNullOrEmpty(request.ViewCount))
        {
            statistic.ViewCount = request.ViewCount;
        }

        if (!string.IsNullOrEmpty(request.SubscriberCount))
        {
            statistic.SubscriberCount = request.SubscriberCount;
        }

        if (!string.IsNullOrEmpty(request.VideoCount))
        {
            statistic.VideoCount = request.VideoCount;
        }

        return await _statisticRepository.UpdateAsync(statistic);
    }

    public async Task DeleteStatisticAsync(int id)
    {
        if (!await _statisticRepository.ExistsByIdAsync(id))
        {
            throw new KeyNotFoundException($"Statistic with id {id} not found");
        }

        await _statisticRepository.DeleteAsync(id);
    }
}
