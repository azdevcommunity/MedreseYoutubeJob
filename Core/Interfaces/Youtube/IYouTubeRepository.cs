using YoutubeApiSynchronize.Core.Entities;

namespace YoutubeApiSynchronize.Core.Interfaces.Youtube;

public interface IYouTubeRepository
{
    Task<Entities.Video?> GetVideoByIdAsync(string videoId);
    Task<Entities.Playlist?> GetPlaylistByIdAsync(string playlistId);
    Task<ChannelStat?> GetChannelStatAsync();
    Task<bool> VideoExistsAsync(string videoId);
    Task<bool> PlaylistExistsAsync(string playlistId);
    Task<bool> PlaylistVideoExistsAsync(string videoId, string playlistId);
    Task<int> GetPlaylistVideoCountAsync(string playlistId);
    Task AddVideoAsync(Entities.Video video);
    Task AddPlaylistAsync(Entities.Playlist playlist);
    Task AddPlaylistVideoAsync(PlaylistVideo playlistVideo);
    Task UpdateVideoAsync(Entities.Video video);
    Task UpdatePlaylistAsync(Entities.Playlist playlist);
    Task UpdateChannelStatAsync(ChannelStat channelStat);
    Task UpdatePlaylistVideoCountsAsync();
    Task SaveChangesAsync();
}
