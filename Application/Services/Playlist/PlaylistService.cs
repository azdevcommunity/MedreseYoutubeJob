using YoutubeApiSynchronize.Application.Dtos.Playlist.Responses;
using YoutubeApiSynchronize.Core.Interfaces.Playlist;

namespace YoutubeApiSynchronize.Application.Services.Playlist;

public class PlaylistService : IPlaylistService
{
    private readonly IPlaylistRepository _playlistRepository;

    public PlaylistService(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    public async Task<List<PlaylistResponse>> GetAllPlaylistsWithSearchAsync(string? search)
    {
        return await _playlistRepository.GetAllWithSearchAsync(search);
    }

    public async Task<PlaylistResponse> GetPlaylistByIdAsync(string playlistId)
    {
        var playlist = await _playlistRepository.GetByPlaylistIdAsync(playlistId);
        
        if (playlist == null)
        {
            throw new KeyNotFoundException($"Playlist not found with id: {playlistId}");
        }

        return playlist;
    }

    public async Task<PlaylistResponse> GetByOfVideoAsync(string videoId)
    {
        var playlists = await _playlistRepository.GetAllByVideoIdAsync(videoId);
        
        if (playlists == null || !playlists.Any())
        {
            throw new KeyNotFoundException($"No playlists found for video id: {videoId}");
        }

        // Return the last playlist (matching Java's getLast() behavior)
        return playlists.Last();
    }
}
