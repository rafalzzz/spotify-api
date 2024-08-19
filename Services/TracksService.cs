using Microsoft.Extensions.Options;
using System.Text.Json;
using FluentValidation;
using SpotifyApi.Classes;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface ITracksService
    {
        Result<SearchTracksParams> ValidateTracksRequestLogin(SearchTracksParams requestParams);
        Task<Result<IEnumerable<Track>>> GetTracksFromItunesApi(SearchTracksParams validParams);
    }

    public class TracksService(
        IRequestValidatorService requestValidatorService,
        IValidator<SearchTracksParams> searchTracksParamsValidator,
        IOptions<ServiceSettings> options,
        HttpClient httpClient
        ) : ITracksService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<SearchTracksParams> _searchTracksParamsValidator = searchTracksParamsValidator;
        private readonly ServiceSettings _serviceSettings = options.Value;
        private readonly HttpClient _httpClient = httpClient;

        public Result<SearchTracksParams> ValidateTracksRequestLogin(SearchTracksParams requestParams)
        {
            var validationResult = _requestValidatorService.ValidateRequest(
                requestParams,
                _searchTracksParamsValidator
            );

            return validationResult.IsSuccess
                ? Result<SearchTracksParams>.Success(requestParams)
                : Result<SearchTracksParams>.Failure(
                    new Error(ErrorType.Validation, validationResult.Error.Description)
                );
        }

        public async Task<Result<IEnumerable<Track>>> GetTracksFromItunesApi(SearchTracksParams validParams)
        {
            var url = $"{_serviceSettings.ItunesApi}/search?term={validParams.Term}&entity={validParams.Entity}&limit={validParams.Limit}&offset={validParams.Offset}";
            var response = await _httpClient.GetAsync(url);

            Console.WriteLine(url);
            Console.WriteLine(response);

            if (!response.IsSuccessStatusCode)
            {
                return Result<IEnumerable<Track>>.Failure(Error.ApiItunesError);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseString);

            var tracks = jsonDocument.RootElement.GetProperty("results")
                .EnumerateArray()
                .Select(trackJson => new Track
                {
                    TrackName = trackJson.GetProperty("trackName").GetString(),
                    ArtistName = trackJson.GetProperty("artistName").GetString(),
                })
                .ToList();


            return Result<IEnumerable<Track>>.Success(tracks);
        }
    }
}