using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IPlaylistService
    {
        Result<Playlist> CreatePlaylist(CreatePlaylist createPlaylistDto);
    }

    public class PlaylistService(
        SpotifyDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        IErrorHandlingService errorHandlingService
    ) : IPlaylistService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IPasswordHasherService _passwordHasherService = passwordHasherService;
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