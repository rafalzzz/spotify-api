using Microsoft.AspNetCore.Mvc;
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
        ActionResult HandlePlaylistEditionError(Error err);
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

        public ActionResult HandlePlaylistEditionError(Error err)
        {
            return err.Type switch
            {
                ErrorType.Validation => new BadRequestObjectResult(err),
                ErrorType.NotFound => new NotFoundObjectResult(err),
                ErrorType.Unauthorized => new UnauthorizedObjectResult(err),
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}
