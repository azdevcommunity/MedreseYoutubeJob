using Microsoft.AspNetCore.Mvc;
using YoutubeApiSynchronize.Application.Dtos.ContactUs.Requests;
using YoutubeApiSynchronize.Application.Dtos.ContactUs.Responses;
using YoutubeApiSynchronize.Application.Services.ContactUs;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.WebAPI.Controllers;

[ApiController]
[Route("/api/contact")]
public class ContactUsController : ControllerBase
{
    private readonly IContactUsService _contactUsService;
    private readonly ILogger _logger;

    public ContactUsController(IContactUsService contactUsService, ILogger logger)
    {
        _contactUsService = contactUsService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ContactUsResponse>> CreateContact([FromBody] CreateContactUsRequest request)
    {
        _logger.Information("CreateContact endpoint called");
        var contact = await _contactUsService.CreateAsync(request);
        return Ok(contact);
    }

    [HttpGet]
    public async Task<ActionResult<PagedContactUsResponse>> GetAllContacts(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortDir = "DESC")
    {
        _logger.Information("GetAllContacts endpoint called with page: {Page}, size: {Size}", page, size);
        var contacts = await _contactUsService.GetAllContactsAsync(page, size, sortBy, sortDir);
        return Ok(contacts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContactUsResponse>> GetContactById(int id)
    {
        _logger.Information("GetContactById endpoint called with id: {Id}", id);
        var contact = await _contactUsService.GetByIdAsync(id);
        return Ok(contact);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(int id)
    {
        _logger.Information("DeleteContact endpoint called with id: {Id}", id);
        await _contactUsService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPut("update-batch")]
    public async Task<IActionResult> UpdateBatch([FromBody] List<int> contacts)
    {
        _logger.Information("UpdateBatch endpoint called with {Count} contacts", contacts.Count);
        await _contactUsService.UpdateBatchAsync(contacts);
        return Ok();
    }
}