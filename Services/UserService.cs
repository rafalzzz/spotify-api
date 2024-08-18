using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IUserService
    {
        Task<Result<bool>> UserExists(string email, string nickname);
        Result<User> CreateUser(RegisterUser registerUserDto);
        Result<User> VerifyUser(LoginUser loginUserDto);
        public Result<User> GetUserByEmail(string email);
        Result<User> GetUserByLogin(string login);
        Result<bool> SavePasswordResetToken(string token, User user);
        Result<bool> ChangeUserPassword(User user, string token, string password);
    }

    public class UserService(
        SpotifyDbContext dbContext,
        IPasswordHasherService passwordHasherService
    ) : IUserService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IPasswordHasherService _passwordHasherService = passwordHasherService;

        public async Task<Result<bool>> UserExists(string email, string nickname)
        {
            try
            {
                var exists = await _dbContext.UserExists(email, nickname);
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(
                    new Error(ErrorType.Database, "Database error: " + ex.Message)
                );
            }
        }

        public Result<User> CreateUser(RegisterUser registerUserDto)
        {
            try
            {
                var passwordHash = _passwordHasherService.Hash(registerUserDto.Password);

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

                return Result<User>.Success(newUser);
            }
            catch (Exception ex)
            {
                return Result<User>.Failure(
                    new Error(ErrorType.Database, "Database error: " + ex.Message)
                );
            }
        }

        public Result<User> GetUserByEmail(string email)
        {
            try
            {
                User? user = _dbContext.Users.FirstOrDefault(user => user.Email == email);

                if (user is null)
                {
                    return Result<User>.Failure(Error.WrongLogin);
                }

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User>.Failure(
                    new Error(ErrorType.Database, "Database error: " + ex.Message)
                );
            }
        }

        private Result<User> GetUserByNickname(string nickname)
        {
            try
            {
                User? user = _dbContext.Users.FirstOrDefault(user => user.Nickname == nickname);

                if (user is null)
                {
                    return Result<User>.Failure(Error.WrongLogin);
                }

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User>.Failure(
                    new Error(ErrorType.Database, "Database error: " + ex.Message)
                );
            }
        }

        private Result<bool> VerifyUserPassword(string userPassword, string password)
        {
            try
            {
                var result = _passwordHasherService.Verify(userPassword, password);
                return Result<bool>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(
                    new Error(ErrorType.PasswordHashing, "Password hashing error: " + ex.Message)
                );
            }
        }

        public Result<User> GetUserByLogin(string login)
        {
            var userResult = login.Contains('@') ? GetUserByEmail(login) : GetUserByNickname(login);

            return userResult.IsSuccess ? Result<User>.Success(userResult.Value!) :
                Result<User>.Failure(userResult.Error);
        }

        public Result<User> VerifyUser(LoginUser loginUserDto)
        {
            var userResult = GetUserByLogin(loginUserDto.Login);

            if (!userResult.IsSuccess)
            {
                return Result<User>.Failure(userResult.Error);
            }

            var passwordResult = VerifyUserPassword(userResult.Value.Password, loginUserDto.Password);

            if (!passwordResult.IsSuccess)
            {
                return Result<User>.Failure(passwordResult.Error);
            }

            if (!passwordResult.Value)
            {
                return Result<User>.Failure(Error.WrongPassword);
            }

            return Result<User>.Success(userResult.Value);
        }

        public Result<bool> SavePasswordResetToken(string token, User user)
        {
            try
            {
                user.PasswordResetToken = token;
                _dbContext.SaveChanges();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error changing user password. Time: {DateTime.Now}. Error message: {ex.Message}");
                return Result<bool>.Failure(new Error(ErrorType.Database, "Database error: " + ex.Message));
            }
        }

        public Result<bool> ChangeUserPassword(User user, string token, string password)
        {
            var isPasswordUserResetTokenCorrect = user.PasswordResetToken == token;

            if (!isPasswordUserResetTokenCorrect)
            {
                return Result<bool>.Failure(Error.InvalidToken);
            }

            try
            {
                var passwordHash = _passwordHasherService.Hash(password);
                user.Password = passwordHash;
                user.PasswordResetToken = "";

                _dbContext.SaveChanges();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error changing user password. Time: {DateTime.Now}. Error message: {ex.Message}");
                return Result<bool>.Failure(new Error(ErrorType.Database, "Database error: " + ex.Message));
            }
        }
    }
}