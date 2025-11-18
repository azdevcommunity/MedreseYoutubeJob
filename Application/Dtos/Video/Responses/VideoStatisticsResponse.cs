namespace YoutubeApiSynchronize.Application.Dtos.Video.Responses;

public class VideoStatisticsResponse
{
    public long VideoCount { get; set; }
    public long ViewCount { get; set; }
    public long PlaylistCount { get; set; }
    public long SubscriberCount { get; set; }
}
