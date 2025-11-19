using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Core.Entities;
using YoutubeApiSynchronize.Core.Interfaces.ContactUs;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

public class ContactUsRepository : IContactUsRepository
{
    private readonly MedreseDbContext _context;

    public ContactUsRepository(MedreseDbContext context)
    {
        _context = context;
    }

    public async Task<ContactUs> CreateAsync(ContactUs contactUs)
    {
        await _context.ContactUs.AddAsync(contactUs);
        await _context.SaveChangesAsync();
        return contactUs;
    }

    public async Task<(List<ContactUs> Items, int TotalCount)> GetAllByReadAsync(
        int page, int size, string sortBy, string sortDir, bool read)
    {
        var query = _context.ContactUs.Where(c => c.Read == read);

        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortDir.ToUpper() == "DESC"
            ? query.OrderByDescending(GetSortProperty(sortBy))
            : query.OrderBy(GetSortProperty(sortBy));

        var items = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ContactUs?> GetByIdAsync(int id)
    {
        return await _context.ContactUs.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task DeleteAsync(int id)
    {
        var contactUs = await GetByIdAsync(id);
        if (contactUs != null)
        {
            _context.ContactUs.Remove(contactUs);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<ContactUs>> GetByIdsAsync(List<int> ids)
    {
        return await _context.ContactUs
            .Where(c => ids.Contains(c.Id))
            .ToListAsync();
    }

    public async Task UpdateRangeAsync(List<ContactUs> contacts)
    {
        foreach (var contact in contacts)
        {
            // Only update the Read property to avoid DateTime issues
            _context.Entry(contact).Property(c => c.Read).IsModified = true;
        }
        await _context.SaveChangesAsync();
    }

    private static Expression<Func<ContactUs, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "name" => c => c.Name,
            "email" => c => c.Email,
            "createdat" => c => c.CreatedAt,
            "read" => c => c.Read,
            _ => c => c.CreatedAt
        };
    }
}
