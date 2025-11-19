namespace YoutubeApiSynchronize.Core.Interfaces.Tag;

public interface ITagRepository
{
    Task<List<Entities.Tag>> GetAllAsync();
    Task<Entities.Tag?> GetByIdAsync(int id);
    Task<Entities.Tag> CreateAsync(Entities.Tag tag);
    Task<Entities.Tag> UpdateAsync(Entities.Tag tag);
    Task DeleteAsync(int id);
    
    Task<bool> ExistsByName (string name);
}
