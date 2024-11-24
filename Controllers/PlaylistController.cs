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
        public async Task<ActionResult> CreatePlaylist([FromBody] CreatePlaylist createPlaylistDto)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return await _playlistCreationService.CreatePlaylist(createPlaylistDto, int.Parse(userId))
                .MatchAsync(
                    playlist => Created(string.Empty, playlist),
                    _playlistService.HandlePlaylistRequestError
                );
        }

        [HttpPut("{playlistId}")]
        public async Task<ActionResult> EditPlaylist([FromRoute] int playlistId, [FromBody] EditPlaylist editPlaylistDto)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return await _playlistEditionService.EditPlaylist(playlistId, editPlaylistDto, int.Parse(userId))
                .MatchAsync(
                    Ok,
                    _playlistService.HandlePlaylistRequestError
                );
        }

        [HttpDelete("{playlistId}")]
        public async Task<ActionResult> DeletePlaylist([FromRoute] int playlistId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return await _playlistService.DeletePlaylist(playlistId, int.Parse(userId))
                .MatchAsync(
                    _ => NoContent(),
                    _playlistService.HandlePlaylistRequestError
                );

        }

        [HttpGet("{playlistId}")]
        public async Task<ActionResult> GetPlaylist([FromRoute] int playlistId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return await _playlistService.GetPlaylistWithSongs(playlistId, int.Parse(userId))
                .MatchAsync(
                    Ok,
                    _playlistService.HandlePlaylistRequestError
                );

        }
    }
}