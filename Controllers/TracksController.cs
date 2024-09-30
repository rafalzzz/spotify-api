using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyApi.Classes;
using SpotifyApi.Services;
using SpotifyApi.Utilities;
using SpotifyApi.Variables;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.Tracks)]
    [Authorize]
    public class TracksController(ITracksService tracksService) : ControllerBase
    {
        private readonly ITracksService _tracksService = tracksService;

        [HttpGet("search")]
        public async Task<IActionResult> SearchTracks([FromQuery] SearchTracksParams requestParams)
        {

            return await _tracksService.ValidateTracksRequestLogin(requestParams)
                .BindAsync(_tracksService.GetTracksFromItunesApi)
                .MatchAsync(
                    Ok,
                    error => new ObjectResult("An unexpected error occurred: " + error.Description)
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    }
                );
        }
    }
}
