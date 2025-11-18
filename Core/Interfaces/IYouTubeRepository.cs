using YoutubeApiSynchronize.Core.Entities;

namespace YoutubeApiSynchronize.Core.Interfaces;

public interface IYouTubeRepository
{
    Task<Video?> GetVideoByIdAsync(string videoId);
    Task<Playlist?> GetPlaylistByIdAsync(string playlistId);
    Task<ChannelStat?> GetChannelStatAsync();
    Task<bool> VideoExistsAsync(string videoId);
    Task<bool> PlaylistExistsAsync(string playlistId);
    Task<bool> PlaylistVideoExistsAsync(string videoId, string playlistId);
    Task<int> GetPlaylistVideoCountAsync(string playlistId);
    Task AddVideoAsync(Video video);
    Task AddPlaylistAsync(Playlist playlist);
    Task AddPlaylistVideoAsync(PlaylistVideo playlistVideo);
    Task UpdateVideoAsync(Video video);
    Task UpdatePlaylistAsync(Playlist playlist);
    Task UpdateChannelStatAsync(ChannelStat channelStat);
    Task UpdatePlaylistVideoCountsAsync();
    Task SaveChangesAsync();
}
