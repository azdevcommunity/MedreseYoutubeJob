namespace YoutubeApiSynchronize.Core.Interfaces.ContactUs;

public interface IContactUsRepository
{
    Task<Entities.ContactUs> CreateAsync(Entities.ContactUs contactUs);
    Task<(List<Entities.ContactUs> Items, int TotalCount)> GetAllByReadAsync(int page, int size, string sortBy, string sortDir, bool read);
    Task<Entities.ContactUs?> GetByIdAsync(int id);
    Task DeleteAsync(int id);
    Task<List<Entities.ContactUs>> GetByIdsAsync(List<int> ids);
    Task UpdateRangeAsync(List<Entities.ContactUs> contacts);
}
