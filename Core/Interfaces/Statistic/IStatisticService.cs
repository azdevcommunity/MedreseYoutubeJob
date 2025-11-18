using YoutubeApiSynchronize.Application.Dtos.Statistic.Requests;
using YoutubeApiSynchronize.Application.Dtos.Statistic.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Statistic;

public interface IStatisticService
{
    Task<List<Entities.Statistic>> GetAllStatisticsAsync();
    Task<Entities.Statistic> GetStatisticByIdAsync(int id);
    Task<StatisticResponse> CreateStatisticAsync(CreateStatisticRequest request);
    Task<Entities.Statistic> UpdateStatisticAsync(int id, UpdateStatisticRequest request);
    Task DeleteStatisticAsync(int id);
}
