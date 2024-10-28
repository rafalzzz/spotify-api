using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyApi.Entities;
using SpotifyApi.Responses;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistService
    {
        Result<Playlist> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId);
        Result<Playlist> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId);
        Result<int> DeletePlaylist(int playlistId, int userId);
        Result<bool> AddCollaborator(int playlistId, int collaboratorId, int userId);
        Result<bool> RemoveCollaborator(int playlistId, int collaboratorId, int userId);
        ActionResult HandlePlaylistRequestError(Error err);
    }

    public class PlaylistService(
        SpotifyDbContext dbContext,
        IMapper mapper,
        IErrorHandlingService errorHandlingService,
        IUserService userService
    ) : IPlaylistService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IMapper _mapper = mapper;
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
                var playlist = _dbContext.Playlists
                    .Include(playlist => playlist.Collaborators)
                    .FirstOrDefault(playlist => playlist.Id == id);

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

        private static Result<Playlist> VerifyPlaylistOwner(Playlist playlist, int userId)
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
            .Bind(playlist => VerifyPlaylistOwner(playlist, userId))
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
            .Bind(playlist => VerifyPlaylistOwner(playlist, userId))
            .Bind(DeletePlaylistFromDb);
        }

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
                return HandlePlaylistException<bool>(logErrorAction, exception);
            }
        }

        public Result<bool> AddCollaborator(int playlistId, int collaboratorId, int userId)
        {
            var playlistResult = GetPlaylistById(playlistId);

            if (!playlistResult.IsSuccess)
            {
                return Result<bool>.Failure(playlistResult.Error);
            }

            var playlist = playlistResult.Value;

            return VerifyPlaylistOwner(playlist, userId)
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
                return HandlePlaylistException<bool>(logErrorAction, exception);
            }
        }

        public Result<bool> RemoveCollaborator(int playlistId, int collaboratorId, int userId)
        {
            var playlistResult = GetPlaylistById(playlistId);

            if (!playlistResult.IsSuccess)
            {
                return Result<bool>.Failure(playlistResult.Error);
            }

            var playlist = playlistResult.Value;

            return VerifyPlaylistOwner(playlist, userId)
                .Bind(_ => GetCollaboratorById(collaboratorId))
                .Bind(collaborator => IsCollaboratorAdded(playlist, collaborator))
                .Bind(collaborator => RemoveCollaboratorFromPlaylist(playlist, collaborator));
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