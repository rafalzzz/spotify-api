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
        Task<Result<IEnumerable<Track>>> GetTracksByIds(IEnumerable<int> songIds);
    }

    public class TracksService(
        IRequestValidatorService requestValidatorService,
        IValidator<SearchTracksParams> searchTracksParamsValidator,
        IOptions<ServiceSettings> options,
        HttpClient httpClient,
        IErrorHandlingService errorHandlingService
        ) : ITracksService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<SearchTracksParams> _searchTracksParamsValidator = searchTracksParamsValidator;
        private readonly ServiceSettings _serviceSettings = options.Value;
        private readonly HttpClient _httpClient = httpClient;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

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
            return await FetchTracksFromApi(url, "get tracks from iTunes API");
        }

        public async Task<Result<IEnumerable<Track>>> GetTracksByIds(IEnumerable<int> songIds)
        {
            var idsQuery = string.Join(",", songIds);
            var url = $"{_serviceSettings.ItunesApi}/lookup?id={idsQuery}";
            return await FetchTracksFromApi(url, "get tracks by IDs from iTunes API");
        }

        private async Task<Result<IEnumerable<Track>>> FetchTracksFromApi(string url, string logErrorAction)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);

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
                        TrackName = trackJson.GetProperty("trackName").GetString() ?? "Unknown track",
                        ArtistName = trackJson.GetProperty("artistName").GetString() ?? "Unknown artist",
                    })
                    .ToList();

                return Result<IEnumerable<Track>>.Success(tracks);
            }
            catch (Exception exception)
            {
                var error = _errorHandlingService.HandleError(
                    exception,
                    ErrorType.ApiItunes,
                    logErrorAction,
                    "iTunes API"
                );

                return Result<IEnumerable<Track>>.Failure(error);
            }
        }
    }
}