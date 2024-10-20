using FluentValidation;
using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistEditionService
    {
        Result<EditPlaylist> ValidatePlaylistEdition(EditPlaylist createPlaylistDto);
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
            var editPlaylistResult = _playlistService.EditPlaylist(playlistId, editPlaylistDto, userId);

            return editPlaylistResult.IsSuccess ?
                Result<Playlist>.Success(editPlaylistResult.Value) :
                Result<Playlist>.Failure(editPlaylistResult.Error);
        }
    }
}
