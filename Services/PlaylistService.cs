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
        ActionResult HandlePlaylistRequestError(Error err);
    }

    public class PlaylistService(
        SpotifyDbContext dbContext,
        IErrorHandlingService errorHandlingService
    ) : IPlaylistService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

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

        public Result<Playlist> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId)
        {
            try
            {
                var playlist = _dbContext.Playlists.FirstOrDefault(
                        playlist => playlist.Id == playlistId
                    );

                if (playlist == null)
                {
                    return Result<Playlist>.Failure(Error.NotFound);
                }

                if (playlist.OwnerId != userId)
                {
                    return Result<Playlist>.Failure(Error.Unauthorized);
                }

                playlist.Name = editPlaylistDto.Name ?? playlist.Name;
                playlist.Description = editPlaylistDto.Description ?? playlist.Description;
                playlist.IsPublic = editPlaylistDto.IsPublic ?? playlist.IsPublic;

                _dbContext.SaveChanges();

                return Result<Playlist>.Success(playlist);
            }
            catch (Exception exception)
            {
                var logErrorAction = "edit playlist";
                return HandlePlaylistException<Playlist>(logErrorAction, exception);
            }
        }

        public Result<int> DeletePlaylist(int playlistId, int userId)
        {
            try
            {
                var playlist = _dbContext.Playlists.FirstOrDefault(
                        playlist => playlist.Id == playlistId
                    );

                if (playlist == null)
                {
                    return Result<int>.Failure(Error.NotFound);
                }

                if (playlist.OwnerId != userId)
                {
                    return Result<int>.Failure(Error.Unauthorized);
                }

                _dbContext.Playlists.Remove(playlist);
                _dbContext.SaveChanges();

                return Result<int>.Success(playlistId);
            }
            catch (Exception exception)
            {
                var logErrorAction = "delete playlist";
                return HandlePlaylistException<int>(logErrorAction, exception);
            }
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
                ErrorType.NotFound => new NotFoundObjectResult(err.Description),
                ErrorType.Unauthorized => new UnauthorizedObjectResult(err.Description),
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}