using System.ComponentModel.DataAnnotations;

namespace YoutubeApiSynchronize.Application.Dtos.Statistic.Requests;

public class CreateStatisticRequest
{
    [Required(ErrorMessage = "View Count is required")]
    public string ViewCount { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subscriber Count is required")]
    public string SubscriberCount { get; set; } = string.Empty;

    public bool HiddenSubscriber { get; set; }

    [Required(ErrorMessage = "Video Count is required")]
    public string VideoCount { get; set; } = string.Empty;
}
