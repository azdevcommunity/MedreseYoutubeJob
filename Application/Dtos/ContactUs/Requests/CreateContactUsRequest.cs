using System.ComponentModel.DataAnnotations;

namespace YoutubeApiSynchronize.Application.Dtos.ContactUs.Requests;

public class CreateContactUsRequest
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    public string? Subject { get; set; }

    [Required(ErrorMessage = "Phone is required")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    public string Message { get; set; } = string.Empty;
}
