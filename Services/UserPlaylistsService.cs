using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyApi.Entities;
using SpotifyApi.Responses;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IUserPlaylistsService
    {
        Task<Result<UserPlaylistDto[]>> GetUserPlaylists(int userId);
        ActionResult HandleUserPlaylistsRequestError(Error err);
    }

    public class UserPlaylistsService(
        SpotifyDbContext dbContext,
        IMapper mapper,
        IErrorHandlingService errorHandlingService
    ) : IUserPlaylistsService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IMapper _mapper = mapper;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        private UserPlaylistDto MapPlaylistEntityToUserPlaylistDto(Playlist playlist, int userId)
        {
            return _mapper.Map<UserPlaylistDto>(playlist, opts => opts.Items["UserId"] = userId);
        }

        private async Task<Result<User>> GetUserWithPlaylistsDataById(int id)
        {
            try
            {
                var user = await _dbContext.Users
                    .Include(u => u.CreatedPlaylists)
                    .Include(u => u.CollaboratingPlaylists)
                    .Include(u => u.FavoritePlaylists)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user is null)
                {
                    return Result<User>.Failure(Error.WrongUserId);
                }

                return Result<User>.Success(user);
            }
            catch (Exception exception)
            {
                var logErrorAction = "get user with playlists data by id";
                return _errorHandlingService.HandleDatabaseException<User>(logErrorAction, exception);
            }
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
            return await GetUserWithPlaylistsDataById(userId)
                .ThenBind(GetUserPlaylists);
        }

        public ActionResult HandleUserPlaylistsRequestError(Error err)
        {
            return err.Type switch
            {
                ErrorType.Unauthorized => new UnauthorizedObjectResult(err.Description),
                ErrorType.WrongUserId => new BadRequestObjectResult(err.Description),
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}