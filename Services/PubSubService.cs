namespace YoutubeApiSynchronize.Services;

using ILogger = Serilog.ILogger;

public class PubSubService(ILogger logger)
{
    private const string ChannelId = "UCN22jHS7MPBp38ZWZemt7iQ";
    private const string HubUrl = "https://pubsubhubbub.appspot.com/subscribe";

    public async Task SubscribeToTopic(string payload, string challenge)
    {
        string callbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/Youtube/push";

        string topicUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={ChannelId}";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hub.callback", callbackUrl),
            new KeyValuePair<string, string>("hub.mode", "subscribe"),
            new KeyValuePair<string, string>("hub.topic", topicUrl),
            new KeyValuePair<string, string>("hub.verify", "sync")
        });

        using var client = new HttpClient();
        using var response = await client.PostAsync(HubUrl, content);

        logger.Information(response.IsSuccessStatusCode
            ? "Successfully subscribed to the channel."
            : $"Failed to subscribe. Status code: {response.StatusCode}");
    }

    public async Task UnsubscribeAndUpdateCallbackUrl()
    {
        string newCallbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/Youtube/push";
        string oldCallbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/youtube-pubsub/push";
        string topicUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={ChannelId}";

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
                Console.WriteLine("Unsubscribed successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to unsubscribe. Status code: {unsubscribeResponse.StatusCode}");
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
                Console.WriteLine("Successfully subscribed with new callback URL.");
            }
            else
            {
                Console.WriteLine($"Failed to subscribe. Status code: {subscribeResponse.StatusCode}");
                throw new Exception("Failed to subscribe. Status code: {subscribeResponse.StatusCode}");
            }
        }
    }
}