namespace YoutubeApiSynchronize.Application.Dtos;

public class YouTubeNotificationDto
{
    public string VideoId { get; set; }
    public string ChannelId { get; set; }
    public string Title { get; set; }
    public string VideoUrl { get; set; }
    public string ChannelUrl { get; set; }
    public string Published { get; set; }
    public string Updated { get; set; }
}
