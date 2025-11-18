using System.Text.Json;
using YoutubeApiSynchronize.Core.Entities;
using YoutubeApiSynchronize.Core.Interfaces;
using YoutubeApiSynchronize.Infrastructure.Database;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.Application.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger _logger;
    private readonly MedreseDbContext _context;
    private const string HubUrl = "https://pubsubhubbub.appspot.com/subscribe";

    public NotificationService(ILogger logger, MedreseDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task SubscribeToChannelAsync(string channelId, string callbackUrl)
    {
        string topicUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={channelId}";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hub.callback", callbackUrl),
            new KeyValuePair<string, string>("hub.mode", "subscribe"),
            new KeyValuePair<string, string>("hub.topic", topicUrl),
            new KeyValuePair<string, string>("hub.verify", "sync")
        });

        using var client = new HttpClient();
        using var response = await client.PostAsync(HubUrl, content);

        _logger.Information(response.IsSuccessStatusCode
            ? "Successfully subscribed to the channel."
            : $"Failed to subscribe. Status code: {response.StatusCode}");
    }

    public async Task UnsubscribeAndUpdateCallbackUrlAsync(string oldCallbackUrl, string newCallbackUrl, string channelId)
    {
        string topicUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={channelId}";

        var unsubscribeContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hub.callback", oldCallbackUrl),
            new KeyValuePair<string, string>("hub.mode", "unsubscribe"),
            new KeyValuePair<string, string>("hub.topic", topicUrl),
        });

        using (var client = new HttpClient())
        {
            var unsubscribeResponse = await client.PostAsync(HubUrl, unsubscribeContent);
            if (unsubscribeResponse.IsSuccessStatusCode)
            {
                _logger.Information("Unsubscribed successfully.");
            }
            else
            {
                _logger.Error("Failed to unsubscribe. Status code: {StatusCode}", unsubscribeResponse.StatusCode);
                throw new Exception($"Failed to unsubscribe. Status code: {unsubscribeResponse.StatusCode}");
            }
        }

        var subscribeContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hub.callback", newCallbackUrl),
            new KeyValuePair<string, string>("hub.mode", "subscribe"),
            new KeyValuePair<string, string>("hub.topic", topicUrl),
            new KeyValuePair<string, string>("hub.verify", "sync")
        });

        using (var client = new HttpClient())
        {
            var subscribeResponse = await client.PostAsync(HubUrl, subscribeContent);
            if (subscribeResponse.IsSuccessStatusCode)
            {
                _logger.Information("Successfully subscribed with new callback URL.");
            }
            else
            {
                _logger.Error("Failed to subscribe. Status code: {StatusCode}", subscribeResponse.StatusCode);
                throw new Exception($"Failed to subscribe. Status code: {subscribeResponse.StatusCode}");
            }
        }
    }

    public async Task ProcessNotificationAsync(string notificationData, string? host = null)
    {
        var youtubeNotification = new YouTubeNotification
        {
            NotificationData = notificationData,
            Title = host ?? string.Empty
        };

        await _context.YouTubeNotifications.AddAsync(youtubeNotification);
        await _context.SaveChangesAsync();
    }
}
