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
        try
        {
            _logger.Information("CreateContact endpoint called");
            var contact = await _contactUsService.CreateAsync(request);
            return Ok(contact);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in CreateContact endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedContactUsResponse>> GetAllContacts(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortDir = "DESC")
    {
        try
        {
            _logger.Information("GetAllContacts endpoint called with page: {Page}, size: {Size}", page, size);
            var contacts = await _contactUsService.GetAllContactsAsync(page, size, sortBy, sortDir);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetAllContacts endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContactUsResponse>> GetContactById(int id)
    {
        try
        {
            _logger.Information("GetContactById endpoint called with id: {Id}", id);
            var contact = await _contactUsService.GetByIdAsync(id);
            return Ok(contact);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Contact not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in GetContactById endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(int id)
    {
        try
        {
            _logger.Information("DeleteContact endpoint called with id: {Id}", id);
            await _contactUsService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warning(ex, "Contact not found with id: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in DeleteContact endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("update-batch")]
    public async Task<IActionResult> UpdateBatch([FromBody] List<int> contacts)
    {
        try
        {
            _logger.Information("UpdateBatch endpoint called with {Count} contacts", contacts.Count);
            await _contactUsService.UpdateBatchAsync(contacts);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in UpdateBatch endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
