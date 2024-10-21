using FluentValidation;
using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistEditionService
    {
        Result<EditPlaylist> ValidatePlaylistEdition(EditPlaylist editPlaylistDto);
        Result<Playlist> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId);
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

        public Result<EditPlaylist> ValidatePlaylistEdition(EditPlaylist editPlaylistDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(
                editPlaylistDto,
                _editPlaylistValidator
            );

            return validationResult.IsSuccess ? Result<EditPlaylist>.Success(editPlaylistDto) :
                Result<EditPlaylist>.Failure(validationResult.Error);
        }

        public Result<Playlist> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId)
        {
            return ValidatePlaylistEdition(editPlaylistDto)
            .Bind(editPlaylist => _playlistService.EditPlaylist(playlistId, editPlaylist, userId));
        }
    }
}
