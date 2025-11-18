using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Core.Entities;
using YoutubeApiSynchronize.Core.Interfaces.Statistic;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

public class StatisticRepository : IStatisticRepository
{
    private readonly MedreseDbContext _context;

    public StatisticRepository(MedreseDbContext context)
    {
        _context = context;
    }

    public async Task<List<Statistic>> GetAllAsync()
    {
        return await _context.Statistics.ToListAsync();
    }

    public async Task<Statistic?> GetByIdAsync(int id)
    {
        return await _context.Statistics.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Statistic> CreateAsync(Statistic statistic)
    {
        await _context.Statistics.AddAsync(statistic);
        await _context.SaveChangesAsync();
        return statistic;
    }

    public async Task<Statistic> UpdateAsync(Statistic statistic)
    {
        _context.Statistics.Update(statistic);
        await _context.SaveChangesAsync();
        return statistic;
    }

    public async Task DeleteAsync(int id)
    {
        var statistic = await GetByIdAsync(id);
        if (statistic != null)
        {
            _context.Statistics.Remove(statistic);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await _context.Statistics.AnyAsync(s => s.Id == id);
    }
}
