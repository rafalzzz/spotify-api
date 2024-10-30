using FluentValidation;
using SpotifyApi.Responses;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistCreationService
    {
        Result<PlaylistDto> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId);
    }

    public class PlaylistCreationService(
        IRequestValidatorService requestValidatorService,
        IValidator<CreatePlaylist> createPlaylistValidator,
        IPlaylistService playlistService
    ) : IPlaylistCreationService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<CreatePlaylist> _createPlaylistValidator = createPlaylistValidator;
        private readonly IPlaylistService _playlistService = playlistService;

        public Result<CreatePlaylist> ValidatePlaylistCreation(CreatePlaylist createPlaylistDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(
                createPlaylistDto,
                _createPlaylistValidator
            );

            return validationResult.IsSuccess ? Result<CreatePlaylist>.Success(createPlaylistDto) :
                Result<CreatePlaylist>.Failure(validationResult.Error);
        }

        public Result<PlaylistDto> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId)
        {
            return ValidatePlaylistCreation(createPlaylistDto)
            .Bind(createPlaylist => _playlistService.CreatePlaylist(createPlaylist, userId));
        }
    }
}
