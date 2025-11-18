using YoutubeApiSynchronize.Application.Dtos.ContactUs.Requests;
using YoutubeApiSynchronize.Application.Dtos.ContactUs.Responses;
using YoutubeApiSynchronize.Core.Interfaces.ContactUs;

namespace YoutubeApiSynchronize.Application.Services.ContactUs;

public class ContactUsService : IContactUsService
{
    private readonly IContactUsRepository _contactUsRepository;

    public ContactUsService(IContactUsRepository contactUsRepository)
    {
        _contactUsRepository = contactUsRepository;
    }

    public async Task<ContactUsResponse> CreateAsync(CreateContactUsRequest request)
    {
        var contactUs = new Core.Entities.ContactUs
        {
            Name = request.Name,
            Email = request.Email,
            Subject = request.Subject,
            Phone = request.Phone,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow,
            Read = false
        };

        var createdContactUs = await _contactUsRepository.CreateAsync(contactUs);
        return MapToResponse(createdContactUs);
    }

    public async Task<PagedContactUsResponse> GetAllContactsAsync(int page, int size, string sortBy, string sortDir)
    {
        var (items, totalCount) = await _contactUsRepository.GetAllByReadAsync(page, size, sortBy, sortDir, false);

        var totalPages = (int)Math.Ceiling(totalCount / (double)size);

        return new PagedContactUsResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = size,
            TotalPages = totalPages,
            HasPreviousPage = page > 0,
            HasNextPage = page < totalPages - 1
        };
    }

    public async Task<ContactUsResponse> GetByIdAsync(int id)
    {
        var contactUs = await _contactUsRepository.GetByIdAsync(id);
        if (contactUs == null)
        {
            throw new KeyNotFoundException($"Contact with id {id} not found");
        }

        return MapToResponse(contactUs);
    }

    public async Task DeleteAsync(int id)
    {
        var contactUs = await _contactUsRepository.GetByIdAsync(id);
        if (contactUs == null)
        {
            throw new KeyNotFoundException($"Contact with id {id} not found");
        }

        await _contactUsRepository.DeleteAsync(id);
    }

    public async Task UpdateBatchAsync(List<int> ids)
    {
        var contacts = await _contactUsRepository.GetByIdsAsync(ids);
        
        foreach (var contact in contacts)
        {
            contact.Read = true;
        }

        await _contactUsRepository.UpdateRangeAsync(contacts);
    }

    private ContactUsResponse MapToResponse(Core.Entities.ContactUs contactUs)
    {
        return new ContactUsResponse
        {
            Id = contactUs.Id,
            Name = contactUs.Name,
            Email = contactUs.Email,
            Subject = contactUs.Subject,
            Phone = contactUs.Phone,
            Message = contactUs.Message,
            CreatedAt = contactUs.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
            Read = contactUs.Read
        };
    }
}
