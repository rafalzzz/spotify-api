using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyApi.Variables;
using SpotifyApi.Requests;
using SpotifyApi.Services;
using SpotifyApi.Utilities;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.Playlist)]
    [Authorize]
    public class PlaylistController(
        IPlaylistCreationService playlistCreationService
    ) : ControllerBase
    {
        private readonly IPlaylistCreationService _playlistCreationService = playlistCreationService;

        [HttpPost]
        public ActionResult Create([FromBody] CreatePlaylist createPlaylistDto)
        {
            return _playlistCreationService.ValidatePlaylistCreation(createPlaylistDto)
                .Bind(_ => _playlistCreationService.CreatePlaylist(createPlaylistDto))
                .Match(
                    playlist => Created(string.Empty, playlist),
                    _playlistCreationService.HandlePlaylistCreationError
                );
        }
    }
}