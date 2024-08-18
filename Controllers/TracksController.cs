using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpotifyApi.Classes;
using SpotifyApi.Services;
using SpotifyApi.Variables;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.Tracks)]
    public class TracksController(
        HttpClient httpClient,
        IOptions<ServiceSettings> options,
        IRequestValidatorService requestValidatorService,
        IValidator<SearchTracksParams> searchTracksParamsValidator
        ) : ControllerBase
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ServiceSettings _serviceSettings = options.Value;
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<SearchTracksParams> _searchTracksParamsValidator
         = searchTracksParamsValidator;

        public async Task<IActionResult> SearchTracks([FromQuery] SearchTracksParams requestParams)
        {

            /* var validationResult = _requestValidatorService.ValidateRequest(requestParams, _searchTracksParamsValidator);
            if (validationResult is BadRequestObjectResult)
            {
                return validationResult;
            }

            string url = $"{_serviceSettings.ItunesApi}/search?term={requestParams.Term}&entity={requestParams.Entity}&limit={requestParams.Limit}&offset={requestParams.Offset}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error calling iTunes API");
            }

            var content = await response.Content.ReadAsStringAsync(); */
            return Ok();
        }
    }
}
