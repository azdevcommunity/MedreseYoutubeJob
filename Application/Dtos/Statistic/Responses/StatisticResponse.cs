namespace YoutubeApiSynchronize.Application.Dtos.Statistic.Responses;

public class StatisticResponse
{
    public int Id { get; set; }
    public string? ViewCount { get; set; }
    public string? PlatformName { get; set; }
    public string? SubscriberCount { get; set; }
    public string? VideoCount { get; set; }
}
