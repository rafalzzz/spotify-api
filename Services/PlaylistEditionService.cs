using FluentValidation;
using SpotifyApi.Requests;
using SpotifyApi.Responses;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistEditionService
    {
        Task<Result<PlaylistDto>> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId);
    }

    public class PlaylistEditionService(
        IRequestValidatorService requestValidatorService,
        IValidator<EditPlaylist> editPlaylistValidator,
        IPlaylistService playlistService
    ) : IPlaylistEditionService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<EditPlaylist> _editPlaylistValidator = editPlaylistValidator;
        private readonly IPlaylistService _playlistService = playlistService;

        private Result<EditPlaylist> ValidatePlaylistEdition(EditPlaylist editPlaylistDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(
                editPlaylistDto,
                _editPlaylistValidator
            );

            return validationResult.IsSuccess ? Result<EditPlaylist>.Success(editPlaylistDto) :
                Result<EditPlaylist>.Failure(validationResult.Error);
        }

        public async Task<Result<PlaylistDto>> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId)
        {
            return await ValidatePlaylistEdition(editPlaylistDto)
            .BindAsync(editPlaylist => _playlistService.EditPlaylist(playlistId, editPlaylist, userId));
        }
    }
}
