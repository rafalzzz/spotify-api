using FluentValidation;
using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistCreationService
    {
        Result<CreatePlaylist> ValidatePlaylistCreation(CreatePlaylist createPlaylistDto);
        Result<Playlist> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId);
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

        public Result<Playlist> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId)
        {
            var createPlaylistResult = _playlistService.CreatePlaylist(createPlaylistDto, userId);

            return createPlaylistResult.IsSuccess ?
                Result<Playlist>.Success(createPlaylistResult.Value) :
                Result<Playlist>.Failure(createPlaylistResult.Error);
        }
    }
}
