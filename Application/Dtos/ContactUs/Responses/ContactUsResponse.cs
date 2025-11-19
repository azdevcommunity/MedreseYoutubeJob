namespace YoutubeApiSynchronize.Application.Dtos.ContactUs.Responses;

public class ContactUsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public bool Read { get; set; }
}
