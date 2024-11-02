using Microsoft.AspNetCore.Mvc;
using SpotifyApi.Entities;
using SpotifyApi.Responses;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistSongsService
    {
        Result<PlaylistDto> AddSong(int playlistId, int songId, int userId);
        Result<PlaylistDto> RemoveSong(int playlistId, int songId, int userId);
        ActionResult HandlePlaylistSongsRequestError(Error err);
    }

    public class PlaylistSongsService(
        SpotifyDbContext dbContext,
        IPlaylistService playlistService,
        IErrorHandlingService errorHandlingService
    ) : IPlaylistSongsService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IPlaylistService _playlistService = playlistService;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        private static Result<Playlist> VerifyUser(Playlist playlist, int userId)
        {
            var isOwner = playlist.OwnerId == userId;

            var isCollaborator = playlist.Collaborators.Any(
                collaborator => collaborator.Id == userId
            );

            return isOwner || isCollaborator ?
                Result<Playlist>.Success(playlist) :
                Result<Playlist>.Failure(Error.Unauthorized);
        }

        private static bool IsSongAddedToArray(Playlist playlist, int newSongId)
        {
            return playlist.SongIds.Any(songId => songId == newSongId);
        }

        private static Result<Playlist> IsSongNotAdded(Playlist playlist, int newSongId)
        {
            return IsSongAddedToArray(playlist, newSongId) ?
                Result<Playlist>.Failure(Error.SongIsAlreadyAdded) :
                Result<Playlist>.Success(playlist);
        }

        private Result<PlaylistDto> AddSongToPlaylist(Playlist playlist, int songId, int userId)
        {
            try
            {
                playlist.SongIds.Add(songId);
                _dbContext.SaveChanges();

                var playlistDto = _playlistService.MapPlaylistEntityToDto(playlist, userId);

                return Result<PlaylistDto>.Success(playlistDto);
            }
            catch (Exception exception)
            {
                var logErrorAction = "add song";
                return _errorHandlingService.HandleDatabaseException<PlaylistDto>(logErrorAction, exception);
            }
        }

        public Result<PlaylistDto> AddSong(int playlistId, int songId, int userId)
        {
            var playlistResult = _playlistService.GetPlaylistById(playlistId);

            if (!playlistResult.IsSuccess)
            {
                return Result<PlaylistDto>.Failure(playlistResult.Error);
            }

            var playlist = playlistResult.Value;

            return VerifyUser(playlist, userId)
                .Bind(_ => IsSongNotAdded(playlist, songId))
                .Bind(_ => AddSongToPlaylist(playlist, songId, userId));
        }

        private static Result<Playlist> IsSongAdded(Playlist playlist, int newSongId)
        {
            return IsSongAddedToArray(playlist, newSongId) ?
                Result<Playlist>.Success(playlist) :
                Result<Playlist>.Failure(Error.SongIsNotAdded);
        }

        private Result<PlaylistDto> RemoveSongFromPlaylist(Playlist playlist, int songId, int userId)
        {
            try
            {
                playlist.SongIds.Remove(songId);
                _dbContext.SaveChanges();

                var playlistDto = _playlistService.MapPlaylistEntityToDto(playlist, userId);

                return Result<PlaylistDto>.Success(playlistDto);
            }
            catch (Exception exception)
            {
                var logErrorAction = "remove song";
                return _errorHandlingService.HandleDatabaseException<PlaylistDto>(logErrorAction, exception);
            }
        }

        public Result<PlaylistDto> RemoveSong(int playlistId, int songId, int userId)
        {
            var playlistResult = _playlistService.GetPlaylistById(playlistId);

            if (!playlistResult.IsSuccess)
            {
                return Result<PlaylistDto>.Failure(playlistResult.Error);
            }

            var playlist = playlistResult.Value;

            return VerifyUser(playlist, userId)
                .Bind(_ => IsSongAdded(playlist, songId))
                .Bind(_ => RemoveSongFromPlaylist(playlist, songId, userId));
        }

        public ActionResult HandlePlaylistSongsRequestError(Error err)
        {
            return err.Type switch
            {
                ErrorType.WrongPlaylistId => new NotFoundObjectResult(err.Description),
                ErrorType.SongIsAlreadyAdded => new BadRequestObjectResult(err.Description),
                ErrorType.SongIsNotAdded => new BadRequestObjectResult(err.Description),
                ErrorType.Unauthorized => new UnauthorizedObjectResult(err.Description),
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}