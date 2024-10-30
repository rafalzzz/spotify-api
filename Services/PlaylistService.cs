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
        Result<PlaylistDto> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId);
        Result<Playlist> GetPlaylistById(int id);
        Result<PlaylistDto> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId);
        Result<bool> DeletePlaylist(int playlistId, int userId);
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

        private PlaylistDto MapPlaylistEntityToDto(Playlist playlist, int userId)
        {
            return _mapper.Map<PlaylistDto>(playlist, opts => opts.Items["UserId"] = userId);
        }

        public Result<PlaylistDto> CreatePlaylist(CreatePlaylist createPlaylistDto, int userId)
        {
            try
            {
                var newPlaylist = _mapper.Map<Playlist>(createPlaylistDto);
                newPlaylist.OwnerId = userId;

                _dbContext.Playlists.Add(newPlaylist);
                _dbContext.SaveChanges();

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

        private Result<PlaylistDto> UpdatePlaylistChanges(Playlist playlist, EditPlaylist editPlaylistDto, int userId)
        {
            try
            {
                playlist.Name = editPlaylistDto.Name ?? playlist.Name;
                playlist.Description = editPlaylistDto.Description ?? playlist.Description;
                playlist.IsPublic = editPlaylistDto.IsPublic ?? playlist.IsPublic;

                _dbContext.SaveChanges();

                var playlistDto = MapPlaylistEntityToDto(playlist, userId);
                return Result<PlaylistDto>.Success(playlistDto);
            }
            catch (Exception exception)
            {
                var logErrorAction = "update playlist";
                return _errorHandlingService.HandleDatabaseException<PlaylistDto>(logErrorAction, exception);
            }
        }

        public Result<PlaylistDto> EditPlaylist(int playlistId, EditPlaylist editPlaylistDto, int userId)
        {
            return GetPlaylistById(playlistId)
            .Bind(playlist => VerifyPlaylistOwner(playlist, userId))
            .Bind(playlist => UpdatePlaylistChanges(playlist, editPlaylistDto, userId));
        }

        private Result<bool> DeletePlaylistFromDb(Playlist playlist)
        {
            try
            {
                _dbContext.Playlists.Remove(playlist);
                _dbContext.SaveChanges();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "delete playlist";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public Result<bool> DeletePlaylist(int playlistId, int userId)
        {
            return GetPlaylistById(playlistId)
            .Bind(playlist => VerifyPlaylistOwner(playlist, userId))
            .Bind(DeletePlaylistFromDb);
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