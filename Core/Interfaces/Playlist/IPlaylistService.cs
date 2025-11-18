using YoutubeApiSynchronize.Application.Dtos.Playlist.Responses;

namespace YoutubeApiSynchronize.Core.Interfaces.Playlist;

public interface IPlaylistService
{
    Task<List<PlaylistResponse>> GetAllPlaylistsWithSearchAsync(string? search);
    Task<PlaylistResponse> GetPlaylistByIdAsync(string playlistId);
    Task<PlaylistResponse> GetByOfVideoAsync(string videoId);
}
