using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using SpotifyApi.Services;
using SpotifyApi.Utilities;
using SpotifyApi.Variables;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.UserPlaylist)]
    [Authorize]
    public class UserPlaylistsController(
        IUserPlaylistsService userPlaylistsService
    ) : ControllerBase
    {
        private readonly IUserPlaylistsService _userPlaylistsService = userPlaylistsService;

        [HttpGet()]
        public async Task<ActionResult> GetUserPlaylists()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return await _userPlaylistsService.GetUserPlaylists(int.Parse(userId))
                .MatchAsync(
                    Ok,
                    _userPlaylistsService.HandleUserPlaylistsRequestError
                );

        }
    }
}