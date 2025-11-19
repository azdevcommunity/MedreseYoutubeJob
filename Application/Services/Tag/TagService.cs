using YoutubeApiSynchronize.Application.Dtos.Tag.Requests;
using YoutubeApiSynchronize.Application.Dtos.Tag.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Tag;

namespace YoutubeApiSynchronize.Application.Services.Tag;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<List<TagResponse>> GetAllAsync()
    {
        var tags = await _tagRepository.GetAllAsync();
        return tags.Select(MapToResponse).ToList();
    }

    public async Task<TagResponse> GetByIdAsync(int id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with id {id} not found");
        }

        return MapToResponse(tag);
    }

    public async Task<TagResponse> CreateAsync(CreateTagRequest request)
    {
        if (await _tagRepository.ExistsByName(request.Name))
        {
            throw new InvalidOperationException($"Tag with name {request.Name} already exists");
        }
        
        var tag = new Core.Entities.Tag
        {
            Name = request.Name,
            Description = request.Description
        };

        var createdTag = await _tagRepository.CreateAsync(tag);
        return MapToResponse(createdTag);
    }

    public async Task<TagResponse> UpdateAsync(int id, UpdateTagRequest request)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with id {id} not found");
        }

        tag.Name = request.Name;
        tag.Description = request.Description;

        var updatedTag = await _tagRepository.UpdateAsync(tag);
        return MapToResponse(updatedTag);
    }

    public async Task DeleteAsync(int id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with id {id} not found");
        }

        await _tagRepository.DeleteAsync(id);
    }

    private TagResponse MapToResponse(Core.Entities.Tag tag)
    {
        return new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description
        };
    }
}
