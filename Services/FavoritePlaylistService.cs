using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyApi.Entities;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IFavoritePlaylistService
    {
        Result<bool> AddPlaylistToFavorites(int playlistId, int userId);
        Result<bool> RemovePlaylistFromFavorites(int playlistId, int userId);
        ActionResult HandleFavoritePlaylistRequestError(Error err);
    }

    public class FavoritePlaylistService(
        SpotifyDbContext dbContext,
        IPlaylistService playlistService,
        IErrorHandlingService errorHandlingService
    ) : IFavoritePlaylistService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IPlaylistService _playlistService = playlistService;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        private Result<Playlist> GetFavoritePlaylistById(int id)
        {
            var playlistResult = _playlistService.GetPlaylistById(id);

            if (playlistResult.IsSuccess)
            {
                var playlist = playlistResult.Value;

                if (!playlist.IsPublic)
                {
                    return Result<Playlist>.Failure(Error.PlaylistIsNotPublic);
                }

                return Result<Playlist>.Success(playlist);
            }

            return Result<Playlist>.Failure(playlistResult.Error);
        }

        public Result<User> GetUserById(int id)
        {
            try
            {
                User? user = _dbContext.Users
                    .Include(u => u.FavoritePlaylists)
                    .FirstOrDefault(user => user.Id == id);

                if (user is null)
                {
                    return Result<User>.Failure(Error.WrongUserId);
                }

                return Result<User>.Success(user);
            }
            catch (Exception exception)
            {
                var logErrorAction = "get user with favorite playlists by id";
                return _errorHandlingService.HandleDatabaseException<User>(logErrorAction, exception);
            }
        }

        private static Result<User> CheckIfUserIsOwnerOrCollaborator(User user, Playlist playlist)
        {
            var isOwner = playlist.OwnerId == user.Id;
            var isCollaborator = playlist.Collaborators.Any(c => c.Id == user.Id);

            return isOwner || isCollaborator ? Result<User>.Success(user) :
                Result<User>.Failure(Error.WrongUserId);
        }

        private static bool IsPlaylistAddedToFavorites(User user, Playlist playlist)
        {
            return user.FavoritePlaylists.Contains(playlist);
        }

        private Result<bool> AddToFavorites(User user, Playlist playlist)
        {
            if (IsPlaylistAddedToFavorites(user, playlist))
            {
                return Result<bool>.Failure(Error.PlaylistIsAlreadyAddedToFavorites);
            }

            try
            {
                user.FavoritePlaylists.Add(playlist);
                _dbContext.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "add playlist to favorites";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public Result<bool> AddPlaylistToFavorites(int playlistId, int userId)
        {
            var playlistResult = GetFavoritePlaylistById(playlistId);

            if (!playlistResult.IsSuccess)
            {
                return Result<bool>.Failure(playlistResult.Error);
            }

            var playlist = playlistResult.Value;

            return GetUserById(userId)
                .Bind(user => CheckIfUserIsOwnerOrCollaborator(user, playlist))
                .Bind(user => AddToFavorites(user, playlist));
        }

        private Result<bool> RemoveFromFavorites(User user, Playlist playlist)
        {
            if (!IsPlaylistAddedToFavorites(user, playlist))
            {
                return Result<bool>.Failure(Error.PlaylistIsNotAddedToFavorites);
            }

            try
            {
                user.FavoritePlaylists.Remove(playlist);
                _dbContext.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "remove playlist from favorites";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public Result<bool> RemovePlaylistFromFavorites(int playlistId, int userId)
        {
            var playlistResult = GetFavoritePlaylistById(playlistId);

            if (!playlistResult.IsSuccess)
            {
                return Result<bool>.Failure(playlistResult.Error);
            }

            var playlist = playlistResult.Value;

            return GetUserById(userId)
                .Bind(user => RemoveFromFavorites(user, playlist));
        }

        public ActionResult HandleFavoritePlaylistRequestError(Error err)
        {
            return err.Type switch
            {
                ErrorType.WrongPlaylistId => new NotFoundObjectResult(err.Description),
                ErrorType.PlaylistIsNotPublic => new BadRequestObjectResult(err.Description),
                ErrorType.WrongUserId => new NotFoundObjectResult(err.Description),
                ErrorType.PlaylistIsAlreadyAddedToFavorites => new BadRequestObjectResult(err.Description),
                ErrorType.PlaylistIsNotAddedToFavorites => new BadRequestObjectResult(err.Description),
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}