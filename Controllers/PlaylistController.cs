using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyApi.Variables;
using SpotifyApi.Requests;
using SpotifyApi.Services;
using SpotifyApi.Utilities;
using System.IdentityModel.Tokens.Jwt;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.Playlist)]
    [Authorize]
    public class PlaylistController(
        IPlaylistCreationService playlistCreationService,
        IPlaylistEditionService playlistEditionService
    ) : ControllerBase
    {
        private readonly IPlaylistCreationService _playlistCreationService = playlistCreationService;
        private readonly IPlaylistEditionService _playlistEditionService = playlistEditionService;

        [HttpPost("create")]
        public ActionResult Create([FromBody] CreatePlaylist createPlaylistDto)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return _playlistCreationService.ValidatePlaylistCreation(createPlaylistDto)
                .Bind(_ => _playlistCreationService.CreatePlaylist(createPlaylistDto, int.Parse(userId)))
                .Match(
                    playlist => Created(string.Empty, playlist),
                    _playlistCreationService.HandlePlaylistCreationError
                );
        }

        [HttpPut("edit/{playlistId}")]
        public IActionResult EditPlaylist([FromRoute] int playlistId, [FromBody] EditPlaylist editPlaylistDto)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return _playlistEditionService.ValidatePlaylistEdition(editPlaylistDto)
                .Bind(_ => _playlistEditionService.EditPlaylist(playlistId, editPlaylistDto, int.Parse(userId)))
                .Match(
                    Ok,
                    _playlistEditionService.HandlePlaylistEditionError
                );
        }
    }
}