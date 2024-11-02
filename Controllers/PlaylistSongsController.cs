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
    public class PlaylistSongsController(
        IPlaylistSongsService playlistSongsService
    ) : ControllerBase
    {
        private readonly IPlaylistSongsService _playlistSongsService = playlistSongsService;

        [HttpPatch("{playlistId}/songs/{songId}")]
        public IActionResult AddCollaborator([FromRoute] int playlistId, [FromRoute] int songId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return _playlistSongsService.AddSong(playlistId, songId, int.Parse(userId))
                .Match(
                    Ok,
                    _playlistSongsService.HandlePlaylistSongsRequestError
                );
        }

        [HttpDelete("{playlistId}/songs/{songId}")]
        public IActionResult RemoveCollaborator([FromRoute] int playlistId, [FromRoute] int songId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return _playlistSongsService.RemoveSong(playlistId, songId, int.Parse(userId))
                .Match(
                    Ok,
                    _playlistSongsService.HandlePlaylistSongsRequestError
                );
        }
    }
}