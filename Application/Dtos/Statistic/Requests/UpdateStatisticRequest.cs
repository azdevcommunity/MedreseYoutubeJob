namespace YoutubeApiSynchronize.Application.Dtos.Statistic.Requests;

public class UpdateStatisticRequest
{
    public string? PlatformName { get; set; }
    public string? ViewCount { get; set; }
    public string? SubscriberCount { get; set; }
    public string? VideoCount { get; set; }
}
