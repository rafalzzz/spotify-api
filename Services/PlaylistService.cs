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
        Result<Playlist> GetPlaylistById(int id);
        Task<Result<PlaylistDto>> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId);
        Task<Result<bool>> DeletePlaylist(int playlistId, int userId);
        Result<Playlist> VerifyPlaylistOwner(Playlist playlist, int userId);
        Task<Result<UserPlaylistDto[]>> GetUserPlaylists(int userId);
        ActionResult HandlePlaylistRequestError(Error err);
    }

    public class PlaylistService(
        SpotifyDbContext dbContext,
        IUserService userService,
        IMapper mapper,
        IErrorHandlingService errorHandlingService
    ) : IPlaylistService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IUserService _userService = userService;
        private readonly IMapper _mapper = mapper;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        public PlaylistDto MapPlaylistEntityToDto(Playlist playlist, int userId)
        {
            return _mapper.Map<PlaylistDto>(playlist, opts => opts.Items["UserId"] = userId);
        }

        public UserPlaylistDto MapPlaylistEntityToUserPlaylistDto(Playlist playlist, int userId)
        {
            return _mapper.Map<UserPlaylistDto>(playlist, opts => opts.Items["UserId"] = userId);
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

        public Result<Playlist> GetPlaylistById(int id)
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
            .Bind(playlist => VerifyPlaylistOwner(playlist, userId))
            .BindAsync(playlist => UpdatePlaylistChanges(playlist, editPlaylistDto, userId));
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
            .Bind(playlist => VerifyPlaylistOwner(playlist, userId))
            .BindAsync(DeletePlaylistFromDb);
        }

        public Result<UserPlaylistDto[]> GetUserPlaylists(User user)
        {

            var createdAndCollaboratedPlaylists = user.CreatedPlaylists
                .Union(user.CollaboratingPlaylists);

            var publicFavoritePlaylists = user.FavoritePlaylists
                .Where(playlist => playlist.IsPublic);

            var playlists = createdAndCollaboratedPlaylists
                .Union(publicFavoritePlaylists)
                .Distinct()
                .ToList();

            var playlistsDto = playlists
                .Select(playlist => MapPlaylistEntityToUserPlaylistDto(playlist, user.Id))
                .ToArray();

            return Result<UserPlaylistDto[]>.Success(playlistsDto);

        }

        public async Task<Result<UserPlaylistDto[]>> GetUserPlaylists(int userId)
        {
            return await _userService.GetUserWithPlaylistsDataById(userId)
                .ThenBind(GetUserPlaylists);
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