using Microsoft.AspNetCore.Mvc;
using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistService
    {
        Result<Playlist> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId);
        Result<Playlist> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId);
        Result<int> DeletePlaylist(int playlistId, int userId);
        Result<Playlist> AddCollaborator(int playlistId, int collaboratorId, int userId);
        ActionResult HandlePlaylistRequestError(Error err);
    }

    public class PlaylistService(
        SpotifyDbContext dbContext,
        IErrorHandlingService errorHandlingService,
        IUserService userService
    ) : IPlaylistService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;
        private readonly IUserService _userService = userService;

        public Result<Playlist> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId)
        {
            try
            {

                Playlist newPlaylist = new()
                {
                    Name = createPlaylistDto.Name,
                    Description = createPlaylistDto.Description,
                    IsPublic = createPlaylistDto.IsPublic,
                    OwnerId = userId
                };

                _dbContext.Playlists.Add(newPlaylist);
                _dbContext.SaveChanges();

                return Result<Playlist>.Success(newPlaylist);
            }
            catch (Exception exception)
            {
                var logErrorAction = "create playlist";
                return HandlePlaylistException<Playlist>(logErrorAction, exception);
            }
        }

        private Result<Playlist> GetPlaylistById(int id)
        {
            try
            {
                var playlist = _dbContext.Playlists.FirstOrDefault(
                        playlist => playlist.Id == id
                    );

                if (playlist is null)
                {
                    return Result<Playlist>.Failure(Error.WrongPlaylistId);
                }

                return Result<Playlist>.Success(playlist);
            }
            catch (Exception exception)
            {
                var logErrorAction = "get playlist by id";
                return HandlePlaylistException<Playlist>(logErrorAction, exception);
            }
        }

        private static Result<Playlist> VerifyUserOwner(Playlist playlist, int userId)
        {
            return playlist.OwnerId == userId ? Result<Playlist>.Success(playlist) :
                Result<Playlist>.Failure(Error.Unauthorized);
        }

        private Result<Playlist> UpdatePlaylistChanges(Playlist playlist, EditPlaylist editPlaylistDto)
        {
            try
            {
                playlist.Name = editPlaylistDto.Name ?? playlist.Name;
                playlist.Description = editPlaylistDto.Description ?? playlist.Description;
                playlist.IsPublic = editPlaylistDto.IsPublic ?? playlist.IsPublic;

                _dbContext.SaveChanges();

                return Result<Playlist>.Success(playlist);
            }
            catch (Exception exception)
            {
                var logErrorAction = "update playlist";
                return HandlePlaylistException<Playlist>(logErrorAction, exception);
            }
        }

        public Result<Playlist> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId)
        {
            return GetPlaylistById(playlistId)
            .Bind(playlist => VerifyUserOwner(playlist, userId))
            .Bind(playlist => UpdatePlaylistChanges(playlist, editPlaylistDto));
        }

        private Result<int> DeletePlaylistFromDb(Playlist playlist)
        {
            try
            {
                _dbContext.Playlists.Remove(playlist);
                _dbContext.SaveChanges();

                return Result<int>.Success(playlist.Id);
            }
            catch (Exception exception)
            {
                var logErrorAction = "delete playlist";
                return HandlePlaylistException<int>(logErrorAction, exception);
            }
        }

        public Result<int> DeletePlaylist(int playlistId, int userId)
        {
            return GetPlaylistById(playlistId)
            .Bind(playlist => VerifyUserOwner(playlist, userId))
            .Bind(DeletePlaylistFromDb);
        }

        private Result<Playlist> AddCollaboratorToPlaylist(Playlist playlist, int collaboratorId)
        {
            var userResult = _userService.GetUserById(collaboratorId);

            if (!userResult.IsSuccess)
            {
                Result<Playlist>.Failure(Error.WrongUserId);
            }

            try
            {
                playlist.Collaborators.Add(userResult.Value);
                _dbContext.SaveChanges();

                return Result<Playlist>.Success(playlist);
            }
            catch (Exception exception)
            {
                var logErrorAction = "add collaborator";
                return HandlePlaylistException<Playlist>(logErrorAction, exception);
            }
        }

        public Result<Playlist> AddCollaborator(int playlistId, int collaboratorId, int userId)
        {
            return GetPlaylistById(playlistId)
                .Bind(playlist => VerifyUserOwner(playlist, userId))
                .Bind(playlist => AddCollaboratorToPlaylist(playlist, collaboratorId));
        }

        private Result<ResultType> HandlePlaylistException<ResultType>(string logErrorAction, Exception exception)
        {
            var error = _errorHandlingService.HandleDatabaseError(
                exception,
                logErrorAction
            );

            return Result<ResultType>.Failure(error);
        }

        public ActionResult HandlePlaylistRequestError(Error err)
        {
            return err.Type switch
            {
                ErrorType.Validation => new BadRequestObjectResult(err),
                ErrorType.WrongPlaylistId => new NotFoundObjectResult(err.Description),
                ErrorType.WrongUserId => new NotFoundObjectResult(err.Description),
                ErrorType.Unauthorized => new UnauthorizedObjectResult(err.Description),
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}