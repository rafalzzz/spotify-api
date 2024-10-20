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
            try
            {
                var url = $"{_serviceSettings.ItunesApi}/search?term={validParams.Term}&entity={validParams.Entity}&limit={validParams.Limit}&offset={validParams.Offset}";
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
                var logErrorAction = "get tracks from iTunes API";
                var initialErrorMessage = "iTunes API";

                var error = _errorHandlingService.HandleError(
                    exception,
                    ErrorType.ApiItunes,
                    logErrorAction,
                    initialErrorMessage
                );

                return Result<IEnumerable<Track>>.Failure(error);
            }
        }
    }
}