using SpotifyApi.Entities;
using SpotifyApi.Utilities;
namespace SpotifyApi.Services
{
    public interface IPlaylistCollaboratorService
    {
        Result<bool> AddCollaborator(int playlistId, int collaboratorId, int userId);
        Result<bool> RemoveCollaborator(int playlistId, int collaboratorId, int userId);
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

        private Result<User> GetCollaboratorById(int collaboratorId)
        {
            var collaboratorResult = _userService.GetUserById(collaboratorId);

            return collaboratorResult.IsSuccess ?
                Result<User>.Success(collaboratorResult.Value) :
                Result<User>.Failure(Error.WrongUserId);
        }

        private static Result<User> IsCollaboratorNotAdded(Playlist playlist, User collaborator)
        {
            return playlist.Collaborators.Any(c => c.Id == collaborator.Id) ?
                Result<User>.Failure(Error.UserIsAlreadyAdded) :
                Result<User>.Success(collaborator);
        }

        private Result<bool> AddCollaboratorToPlaylist(Playlist playlist, User collaborator)
        {
            try
            {
                playlist.Collaborators.Add(collaborator);
                _dbContext.SaveChanges();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "add collaborator";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public Result<bool> AddCollaborator(int playlistId, int collaboratorId, int userId)
        {
            var playlistResult = _playlistService.GetPlaylistById(playlistId);

            if (!playlistResult.IsSuccess)
            {
                return Result<bool>.Failure(playlistResult.Error);
            }

            var playlist = playlistResult.Value;

            return _playlistService.VerifyPlaylistOwner(playlist, userId)
                .Bind(_ => GetCollaboratorById(collaboratorId))
                .Bind(collaborator => IsCollaboratorNotAdded(playlist, collaborator))
                .Bind(collaborator => AddCollaboratorToPlaylist(playlist, collaborator));
        }

        private static Result<User> IsCollaboratorAdded(Playlist playlist, User collaborator)
        {
            return playlist.Collaborators.Any(c => c.Id == collaborator.Id) ?
                Result<User>.Success(collaborator) :
                Result<User>.Failure(Error.UserIsNotAdded);
        }

        private Result<bool> RemoveCollaboratorFromPlaylist(Playlist playlist, User collaborator)
        {
            try
            {
                playlist.Collaborators.Remove(collaborator);
                _dbContext.SaveChanges();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "remove collaborator";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public Result<bool> RemoveCollaborator(int playlistId, int collaboratorId, int userId)
        {
            var playlistResult = _playlistService.GetPlaylistById(playlistId);

            if (!playlistResult.IsSuccess)
            {
                return Result<bool>.Failure(playlistResult.Error);
            }

            var playlist = playlistResult.Value;

            return _playlistService.VerifyPlaylistOwner(playlist, userId)
                .Bind(_ => GetCollaboratorById(collaboratorId))
                .Bind(collaborator => IsCollaboratorAdded(playlist, collaborator))
                .Bind(collaborator => RemoveCollaboratorFromPlaylist(playlist, collaborator));
        }
    }
}