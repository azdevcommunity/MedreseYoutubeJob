namespace YoutubeApiSynchronize.Services;

using ILogger = Serilog.ILogger;

public class PubSubService(ILogger logger)
{
    public async Task SubscribeToTopic(string payload, string challenge)
    {
        string channelId = "UCN22jHS7MPBp38ZWZemt7i";
        string callbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/Youtube/push";

        string hubUrl = "https://pubsubhubbub.appspot.com/subscribe";
        string topicUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={channelId}";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hub.callback", callbackUrl),
            new KeyValuePair<string, string>("hub.mode", "subscribe"),
            new KeyValuePair<string, string>("hub.topic", topicUrl),
            new KeyValuePair<string, string>("hub.verify", "sync")
        });

        using var client = new HttpClient();
        using var response = await client.PostAsync(hubUrl, content);

        logger.Information(response.IsSuccessStatusCode
            ? "Successfully subscribed to the channel."
            : $"Failed to subscribe. Status code: {response.StatusCode}");
    }

    public async Task UnsubscribeAndUpdateCallbackUrl()
    {
        string channelId = "UCN22jHS7MPBp38ZWZemt7i";
        string newCallbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/Youtube/push";
        string oldCallbackUrl = "https://api-ytb.nizamiyyemedresesi.az/api/youtube-pubsub/push";
        string hubUrl = "https://pubsubhubbub.appspot.com/subscribe";
        string topicUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={channelId}";

        // Unsubscribe işlemi (Eski callback URL ile)
        var unsubscribeContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hub.callback", oldCallbackUrl),
            new KeyValuePair<string, string>("hub.mode", "unsubscribe"),
            new KeyValuePair<string, string>("hub.topic", topicUrl),
        });

        using (var client = new HttpClient())
        {
            var unsubscribeResponse = await client.PostAsync(hubUrl, unsubscribeContent);
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

        // Subscribe işlemi (Yeni callback URL ile)
        var subscribeContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hub.callback", newCallbackUrl),
            new KeyValuePair<string, string>("hub.mode", "subscribe"),
            new KeyValuePair<string, string>("hub.topic", topicUrl),
            new KeyValuePair<string, string>("hub.verify", "sync")
        });

        using (var client = new HttpClient())
        {
            var subscribeResponse = await client.PostAsync(hubUrl, subscribeContent);
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