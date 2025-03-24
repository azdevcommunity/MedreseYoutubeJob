namespace YoutubeApiSynchronize.Options;

public class DatabaseSettings
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
    public string Name { get; set; }
    
    public string ConnectionString => $"Host={Host};Port={Port};Database={Name};Username={Username};Password={Password}";
}