using Microsoft.EntityFrameworkCore;
using YoutubeApiSynchronize.Core.Entities;
using YoutubeApiSynchronize.Core.Interfaces.Author;
using YoutubeApiSynchronize.Infrastructure.Persistence.Database;

namespace YoutubeApiSynchronize.Infrastructure.Persistence.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly MedreseDbContext _context;

    public AuthorRepository(MedreseDbContext context)
    {
        _context = context;
    }

    public async Task<List<Author>> GetAllAsync()
    {
        return await _context.Authors.ToListAsync();
    }

    public async Task<Author?> GetByIdAsync(int id)
    {
        return await _context.Authors.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Author> CreateAsync(Author author)
    {
        await _context.Authors.AddAsync(author);
        await _context.SaveChangesAsync();
        return author;
    }

    public async Task<Author> UpdateAsync(Author author)
    {
        _context.Authors.Update(author);
        await _context.SaveChangesAsync();
        return author;
    }

    public async Task DeleteAsync(int id)
    {
        var author = await GetByIdAsync(id);
        if (author != null)
        {
            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasBooksAsync(int authorId)
    {
        // TODO: Implement when AuthorBook junction table is added
        // return await _context.AuthorBooks.AnyAsync(ab => ab.AuthorId == authorId);
        return await Task.FromResult(false);
    }

    public async Task<bool> HasArticlesAsync(int authorId)
    {
        return await _context.Articles.AnyAsync(a => a.AuthorId == authorId);
    }
}
