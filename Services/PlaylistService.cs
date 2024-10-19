using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistService
    {
        Result<Playlist> CreatePlaylist(CreatePlaylist createPlaylistDto);
        Result<Playlist> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId);
    }

    public class PlaylistService(
        SpotifyDbContext dbContext,
        IErrorHandlingService errorHandlingService
    ) : IPlaylistService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        public Result<Playlist> CreatePlaylist(CreatePlaylist createPlaylistDto)
        {
            try
            {

                Playlist newPlaylist = new()
                {
                    Name = createPlaylistDto.Name,
                    Description = createPlaylistDto.Description,
                    IsPublic = createPlaylistDto.IsPublic,
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
                var playlist = _dbContext.Playlists.FirstOrDefault(p => p.Id == playlistId);

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

        private Result<ResultType> HandlePlaylistException<ResultType>(string logErrorAction, Exception exception)
        {
            var error = _errorHandlingService.HandleDatabaseError(
                exception,
                logErrorAction
            );

            return Result<ResultType>.Failure(error);
        }
    }
}