using YoutubeApiSynchronize.Application.Dtos.Playlist.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Playlist;

public interface IPlaylistRepository
{
    Task<List<PlaylistResponse>> GetAllWithSearchAsync(string? search);
    Task<PlaylistResponse?> GetByPlaylistIdAsync(string playlistId);
    Task<List<PlaylistResponse>> GetAllByVideoIdAsync(string videoId);
}
