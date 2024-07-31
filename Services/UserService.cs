using SpotifyApi.Requests;
using SpotifyApi.Entities;

namespace SpotifyApi.Services
{
    public interface IUserService
    {
        Task<bool> UserExists(string email, string nickname);
        int? CreateUser(RegisterUser userDto);
    }

    public class UserService(
        SpotifyDbContext dbContext,
        IPasswordHasher passwordHasher
            ) : IUserService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;

        public async Task<bool> UserExists(string email, string nickname)
        {
            return await _dbContext.UserExists(email, nickname);
        }

        public int? CreateUser(RegisterUser registerUserDto)
        {
            var passwordHash = _passwordHasher.Hash(registerUserDto.Password);

            User newUser = new()
            {
                Email = registerUserDto.Email,
                Password = passwordHash,
                Nickname = registerUserDto.Nickname,
                DateOfBirth = registerUserDto.DateOfBirth,
                Gender = registerUserDto.Gender,
                Offers = registerUserDto.Offers,
                ShareInformation = registerUserDto.ShareInformation,
                Terms = registerUserDto.Terms,
                RefreshToken = "",
                PasswordResetToken = "",
            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            return newUser.Id;
        }
    }
}