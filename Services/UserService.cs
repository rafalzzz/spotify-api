using SpotifyApi.Requests;
using SpotifyApi.Classes;
using SpotifyApi.Entities;
using SpotifyApi.Enums;

namespace SpotifyApi.Services
{
    public interface IUserService
    {
        Task<bool> UserExists(string email, string nickname);
        int? CreateUser(RegisterUser userDto);
        LoginUserResult VerifyUser(LoginUser loginUserDto);
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

        private User? GetUserByEmail(string email)
        {
            var user = _dbContext.Users.FirstOrDefault(user => user.Email == email);
            return user;
        }

        private User? GetUserByNickname(string nickname)
        {
            var user = _dbContext.Users.FirstOrDefault(user => user.Nickname == nickname);
            return user;
        }

        private bool VerifyUserPassword(string userPassword, string password)
        {
            return _passwordHasher.Verify(
                    userPassword,
                    password
                );
        }

        private User? GetUserByLogin(string login)
        {
            bool isEmail = login.Contains("@");

            if (isEmail)
            {
                return GetUserByEmail(login);
            }

            return GetUserByNickname(login);

        }

        public LoginUserResult VerifyUser(LoginUser loginUserDto)
        {
            User? user = GetUserByLogin(loginUserDto.Login);

            if (user is null)
            {
                return new LoginUserResult(LoginUserError.WrongLogin, null);
            }

            bool isPasswordCorrect = VerifyUserPassword(
                    user.Password,
                    loginUserDto.Password
                );

            if (!isPasswordCorrect)
            {
                return new LoginUserResult(LoginUserError.WrongPassword, null);
            }

            return new LoginUserResult(null, user);
        }
    }
}