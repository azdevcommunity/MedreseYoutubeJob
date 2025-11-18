namespace YoutubeApiSynchronize.Core.Interfaces;

public interface INotificationService
{
    Task SubscribeToChannelAsync(string channelId, string callbackUrl);
    Task UnsubscribeAndUpdateCallbackUrlAsync(string oldCallbackUrl, string newCallbackUrl, string channelId);
    Task ProcessNotificationAsync(string notificationData, string? host = null);
}
