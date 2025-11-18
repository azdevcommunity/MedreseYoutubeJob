namespace YoutubeApiSynchronize.Core.Options;

public class CloudinarySettings
{
    public const string Key = "Cloudinary";
    
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}
