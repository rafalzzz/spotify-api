using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using SpotifyApi.Requests;
using SpotifyApi.Services;
using SpotifyApi.Utilities;
using SpotifyApi.Variables;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.Playlist)]
    [Authorize]
    public class PlaylistController(
        IPlaylistCreationService playlistCreationService,
        IPlaylistEditionService playlistEditionService,
        IPlaylistService playlistService
    ) : ControllerBase
    {
        private readonly IPlaylistCreationService _playlistCreationService = playlistCreationService;
        private readonly IPlaylistEditionService _playlistEditionService = playlistEditionService;
        private readonly IPlaylistService _playlistService = playlistService;

        [HttpPost()]
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
                    _playlistService.HandlePlaylistRequestError
                );
        }

        [HttpPut("{playlistId}")]
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
                    _playlistService.HandlePlaylistRequestError
                );
        }

        [HttpDelete("{playlistId}")]
        public IActionResult DeletePlaylist([FromRoute] int playlistId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return _playlistService.DeletePlaylist(playlistId, int.Parse(userId))
            .Match(
                    _ => NoContent(),
                    _playlistService.HandlePlaylistRequestError
                );

        }
    }
}