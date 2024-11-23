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
        PlaylistDto MapPlaylistEntityToDto(Playlist playlist, int userId);
        Task<Result<PlaylistDto>> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId);
        Task<Result<Playlist>> GetPlaylistById(int id);
        Task<Result<PlaylistDto>> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId);
        Task<Result<bool>> DeletePlaylist(int playlistId, int userId);
        Result<Playlist> VerifyPlaylistOwner(Playlist playlist, int userId);
        ActionResult HandlePlaylistRequestError(Error err);
    }

    public class PlaylistService(
        SpotifyDbContext dbContext,
        IMapper mapper,
        IErrorHandlingService errorHandlingService
    ) : IPlaylistService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IMapper _mapper = mapper;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        public PlaylistDto MapPlaylistEntityToDto(Playlist playlist, int userId)
        {
            return _mapper.Map<PlaylistDto>(playlist, opts => opts.Items["UserId"] = userId);
        }

        public async Task<Result<PlaylistDto>> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId)
        {
            try
            {
                var newPlaylist = _mapper.Map<Playlist>(createPlaylistDto);
                newPlaylist.OwnerId = userId;

                await _dbContext.Playlists.AddAsync(newPlaylist);
                await _dbContext.SaveChangesAsync();

                var playlistDto = MapPlaylistEntityToDto(newPlaylist, userId);
                return Result<PlaylistDto>.Success(playlistDto);
            }
            catch (Exception exception)
            {
                var logErrorAction = "create playlist";
                return _errorHandlingService.HandleDatabaseException<PlaylistDto>(logErrorAction, exception);
            }
        }

        public async Task<Result<Playlist>> GetPlaylistById(int id)
        {
            try
            {
                var playlist = await _dbContext.Playlists
                    .Include(playlist => playlist.Collaborators)
                    .Include(playlist => playlist.FavoritedByUsers)
                    .FirstOrDefaultAsync(playlist => playlist.Id == id);

                if (playlist is null)
                {
                    return Result<Playlist>.Failure(Error.WrongPlaylistId);
                }

                return Result<Playlist>.Success(playlist);
            }
            catch (Exception exception)
            {
                var logErrorAction = "get playlist by id";
                return _errorHandlingService.HandleDatabaseException<Playlist>(logErrorAction, exception);
            }
        }

        public Result<Playlist> VerifyPlaylistOwner(Playlist playlist, int userId)
        {
            return playlist.OwnerId == userId ? Result<Playlist>.Success(playlist) :
                Result<Playlist>.Failure(Error.Unauthorized);
        }

        private async Task<Result<PlaylistDto>> UpdatePlaylistChanges(Playlist playlist, EditPlaylist editPlaylistDto, int userId)
        {
            try
            {
                playlist.Name = editPlaylistDto.Name ?? playlist.Name;
                playlist.Description = editPlaylistDto.Description ?? playlist.Description;
                playlist.IsPublic = editPlaylistDto.IsPublic ?? playlist.IsPublic;

                await _dbContext.SaveChangesAsync();

                var playlistDto = MapPlaylistEntityToDto(playlist, userId);
                return Result<PlaylistDto>.Success(playlistDto);
            }
            catch (Exception exception)
            {
                var logErrorAction = "update playlist";
                return _errorHandlingService.HandleDatabaseException<PlaylistDto>(logErrorAction, exception);
            }
        }

        public async Task<Result<PlaylistDto>> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId)
        {
            return await GetPlaylistById(playlistId)
            .ThenBind(playlist => VerifyPlaylistOwner(playlist, userId))
            .ThenBindAsync(playlist => UpdatePlaylistChanges(playlist, editPlaylistDto, userId));
        }

        private async Task<Result<bool>> DeletePlaylistFromDb(Playlist playlist)
        {
            try
            {
                _dbContext.Playlists.Remove(playlist);
                await _dbContext.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "delete playlist";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public async Task<Result<bool>> DeletePlaylist(int playlistId, int userId)
        {
            return await GetPlaylistById(playlistId)
            .ThenBind(playlist => VerifyPlaylistOwner(playlist, userId))
            .ThenBindAsync(DeletePlaylistFromDb);
        }

        public ActionResult HandlePlaylistRequestError(Error err)
        {
            return err.Type switch
            {
                ErrorType.Validation => new BadRequestObjectResult(err),
                ErrorType.WrongPlaylistId => new NotFoundObjectResult(err.Description),
                ErrorType.Unauthorized => new UnauthorizedObjectResult(err.Description),
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}