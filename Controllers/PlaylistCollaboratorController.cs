using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using SpotifyApi.Services;
using SpotifyApi.Utilities;
using SpotifyApi.Variables;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.Playlist)]
    [Authorize]
    public class PlaylistCollaboratorController(
        IPlaylistCollaboratorService playlistCollaboratorService
    ) : ControllerBase
    {
        private readonly IPlaylistCollaboratorService _playlistCollaboratorService = playlistCollaboratorService;

        [HttpPatch("{playlistId}/collaborators/{collaboratorId}")]
        public IActionResult AddCollaborator([FromRoute] int playlistId, [FromRoute] int collaboratorId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return _playlistCollaboratorService.AddCollaborator(playlistId, collaboratorId, int.Parse(userId))
                .Match(
                    _ => Ok(),
                    _playlistCollaboratorService.HandleCollaboratorRequestError
                );
        }

        [HttpDelete("{playlistId}/collaborators/{collaboratorId}")]
        public IActionResult RemoveCollaborator([FromRoute] int playlistId, [FromRoute] int collaboratorId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return _playlistCollaboratorService.RemoveCollaborator(playlistId, collaboratorId, int.Parse(userId))
                .Match(
                    _ => NoContent(),
                    _playlistCollaboratorService.HandleCollaboratorRequestError
                );
        }
    }
}