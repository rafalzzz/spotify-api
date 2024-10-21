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
        IAddCollaboratorService addCollaboratorService,
        IPlaylistService playlistService
    ) : ControllerBase
    {
        private readonly IPlaylistCreationService _playlistCreationService = playlistCreationService;
        private readonly IPlaylistEditionService _playlistEditionService = playlistEditionService;
        private readonly IAddCollaboratorService _addCollaboratorService = addCollaboratorService;
        private readonly IPlaylistService _playlistService = playlistService;

        [HttpPost()]
        public ActionResult Create([FromBody] CreatePlaylist createPlaylistDto)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return _playlistCreationService.CreatePlaylist(createPlaylistDto, int.Parse(userId))
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

            return _playlistEditionService.EditPlaylist(playlistId, editPlaylistDto, int.Parse(userId))
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

        [HttpPost("{playlistId}/collaborators")]
        public IActionResult AddCollaborator([FromRoute] int playlistId, AddCollaborator addCollaboratorDto)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return _addCollaboratorService.AddCollaborator(playlistId, addCollaboratorDto, int.Parse(userId))
                .Match(
                    Ok,
                    _playlistService.HandlePlaylistRequestError
                );
        }
    }
}