using YoutubeApiSynchronize.Application.Dtos.ContactUs.Requests;
using YoutubeApiSynchronize.Application.Dtos.ContactUs.Responses;

namespace YoutubeApiSynchronize.Application.Services.ContactUs;

public interface IContactUsService
{
    Task<ContactUsResponse> CreateAsync(CreateContactUsRequest request);
    Task<PagedContactUsResponse> GetAllContactsAsync(int page, int size, string sortBy, string sortDir);
    Task<ContactUsResponse> GetByIdAsync(int id);
    Task DeleteAsync(int id);
    Task UpdateBatchAsync(List<int> ids);
}
