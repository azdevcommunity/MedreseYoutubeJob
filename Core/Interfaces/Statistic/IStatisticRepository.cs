namespace YoutubeApiSynchronize.Core.Interfaces.Statistic;

public interface IStatisticRepository
{
    Task<List<Entities.Statistic>> GetAllAsync();
    Task<Entities.Statistic?> GetByIdAsync(int id);
    Task<Entities.Statistic> CreateAsync(Entities.Statistic statistic);
    Task<Entities.Statistic> UpdateAsync(Entities.Statistic statistic);
    Task DeleteAsync(int id);
    Task<bool> ExistsByIdAsync(int id);
}
