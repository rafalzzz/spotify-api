using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using SpotifyApi.Services;
using SpotifyApi.Utilities;
using SpotifyApi.Variables;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.FavoritePlaylist)]
    [Authorize]
    public class FavoritesPlaylistsController(
        IFavoritePlaylistService favoritePlaylistServiceService
    ) : ControllerBase
    {
        private readonly IFavoritePlaylistService _favoritePlaylistServiceService = favoritePlaylistServiceService;

        [HttpPatch("{playlistId}")]
        public async Task<ActionResult> AddPlaylistToFavorites([FromRoute] int playlistId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return await _favoritePlaylistServiceService.AddPlaylistToFavorites(playlistId, int.Parse(userId))
                .MatchAsync(
                    _ => Ok(),
                    _favoritePlaylistServiceService.HandleFavoritePlaylistRequestError
                );
        }

        [HttpDelete("{playlistId}")]
        public async Task<ActionResult> RemovePlaylistFromFavorites([FromRoute] int playlistId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            return await _favoritePlaylistServiceService.RemovePlaylistFromFavorites(playlistId, int.Parse(userId))
                .MatchAsync(
                    _ => Ok(),
                    _favoritePlaylistServiceService.HandleFavoritePlaylistRequestError
                );
        }
    }
}