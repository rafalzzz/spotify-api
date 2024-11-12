using Microsoft.AspNetCore.Mvc;
using SpotifyApi.Entities;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistCollaboratorService
    {
        Task<Result<bool>> AddCollaborator(int playlistId, int collaboratorId, int userId);
        Task<Result<bool>> RemoveCollaborator(int playlistId, int collaboratorId, int userId);
        ActionResult HandleCollaboratorRequestError(Error err);
    }

    public class PlaylistCollaboratorService(
        SpotifyDbContext dbContext,
        IPlaylistService playlistService,
        IErrorHandlingService errorHandlingService,
        IUserService userService
    ) : IPlaylistCollaboratorService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IPlaylistService _playlistService = playlistService;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;
        private readonly IUserService _userService = userService;

        private async Task<Result<User>> GetCollaboratorById(int collaboratorId)
        {
            var collaboratorResult = await _userService.GetUserById(collaboratorId);

            return collaboratorResult.IsSuccess ?
                Result<User>.Success(collaboratorResult.Value) :
                Result<User>.Failure(Error.WrongUserId);
        }

        private static bool IsCollaboratorAddedToArray(Playlist playlist, User collaborator)
        {
            return playlist.Collaborators.Any(c => c.Id == collaborator.Id);
        }

        private static Result<User> IsCollaboratorNotAdded(Playlist playlist, User collaborator)
        {
            return IsCollaboratorAddedToArray(playlist, collaborator) ?
                Result<User>.Failure(Error.UserIsAlreadyAdded) :
                Result<User>.Success(collaborator);
        }

        private async Task<Result<bool>> AddCollaboratorToPlaylist(Playlist playlist, User collaborator)
        {
            try
            {
                playlist.Collaborators.Add(collaborator);
                await _dbContext.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "add collaborator";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public async Task<Result<bool>> AddCollaborator(int playlistId, int collaboratorId, int userId)
        {
            var playlistResult = await _playlistService.GetPlaylistById(playlistId);

            if (!playlistResult.IsSuccess)
            {
                return Result<bool>.Failure(playlistResult.Error);
            }

            var playlist = playlistResult.Value;

            return await _playlistService.VerifyPlaylistOwner(playlist, userId)
                .BindAsync(_ => GetCollaboratorById(collaboratorId))
                .ThenBind(collaborator => IsCollaboratorNotAdded(playlist, collaborator))
                .ThenBindAsync(collaborator => AddCollaboratorToPlaylist(playlist, collaborator));
        }

        private static Result<User> IsCollaboratorAdded(Playlist playlist, User collaborator)
        {
            return IsCollaboratorAddedToArray(playlist, collaborator) ?
                Result<User>.Success(collaborator) :
                Result<User>.Failure(Error.UserIsNotAdded);
        }

        private async Task<Result<bool>> RemoveCollaboratorFromPlaylist(Playlist playlist, User collaborator)
        {
            try
            {
                playlist.Collaborators.Remove(collaborator);
                await _dbContext.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "remove collaborator";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public async Task<Result<bool>> RemoveCollaborator(int playlistId, int collaboratorId, int userId)
        {
            var playlistResult = await _playlistService.GetPlaylistById(playlistId);

            if (!playlistResult.IsSuccess)
            {
                return Result<bool>.Failure(playlistResult.Error);
            }

            var playlist = playlistResult.Value;

            return await _playlistService.VerifyPlaylistOwner(playlist, userId)
                .BindAsync(_ => GetCollaboratorById(collaboratorId))
                .ThenBind(collaborator => IsCollaboratorAdded(playlist, collaborator))
                .ThenBindAsync(collaborator => RemoveCollaboratorFromPlaylist(playlist, collaborator));
        }

        public ActionResult HandleCollaboratorRequestError(Error err)
        {
            return err.Type switch
            {
                ErrorType.WrongPlaylistId => new NotFoundObjectResult(err.Description),
                ErrorType.WrongUserId => new NotFoundObjectResult(err.Description),
                ErrorType.UserIsAlreadyAdded => new BadRequestObjectResult(err.Description),
                ErrorType.UserIsNotAdded => new BadRequestObjectResult(err.Description),
                ErrorType.Unauthorized => new UnauthorizedObjectResult(err.Description),
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}