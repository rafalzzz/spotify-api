using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpotifyApi.Classes;
using SpotifyApi.Enums;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route("/")]
    public class TracksController(HttpClient httpClient, IOptions<ServiceSettings> options) : ControllerBase
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ServiceSettings _serviceSettings = options.Value;

        public async Task<IActionResult> SearchMusic(
            [FromQuery] string term,
            [FromQuery] Entity entity,
            [FromQuery] int limit,
            [FromQuery] int offset)
        {
            Console.WriteLine(entity);
            string url = $"{_serviceSettings.ItunesApi}/search?term={term}&entity={entity}&limit={limit}&offset={offset}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error calling iTunes API");
            }

            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }
    }
}
