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
        public async Task<ActionResult> AddCollaborator([FromRoute] int playlistId, [FromRoute] int songId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return await _playlistSongsService.AddSong(playlistId, songId, int.Parse(userId))
                .MatchAsync(
                    Ok,
                    _playlistSongsService.HandlePlaylistSongsRequestError
                );
        }

        [HttpDelete("{playlistId}/songs/{songId}")]
        public async Task<ActionResult> RemoveCollaborator([FromRoute] int playlistId, [FromRoute] int songId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return await _playlistSongsService.RemoveSong(playlistId, songId, int.Parse(userId))
                .MatchAsync(
                    Ok,
                    _playlistSongsService.HandlePlaylistSongsRequestError
                );
        }
    }
}