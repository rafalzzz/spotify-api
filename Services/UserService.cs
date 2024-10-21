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
        public Result<User> GetUserById(int id);
        Result<User> GetUserByLogin(string login);
        Result<bool> SavePasswordResetToken(string token, User user);
        Result<bool> ChangeUserPassword(User user, string token, string password);
        Result<bool> SaveUserRefreshToken(User user, string refreshToken);
    }

    public class UserService(
        SpotifyDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        IErrorHandlingService errorHandlingService
    ) : IUserService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IPasswordHasherService _passwordHasherService = passwordHasherService;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        public async Task<Result<bool>> UserExists(string email, string nickname)
        {
            try
            {
                var exists = await _dbContext.UserExists(email, nickname);
                return Result<bool>.Success(exists);
            }
            catch (Exception exception)
            {
                var logErrorAction = "check if user exists";
                return HandleUserException<bool>(logErrorAction, exception);
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
            catch (Exception exception)
            {
                var logErrorAction = "create user";
                return HandleUserException<User>(logErrorAction, exception);
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
            catch (Exception exception)
            {
                var logErrorAction = "get user by email";
                return HandleUserException<User>(logErrorAction, exception);
            }
        }

        public Result<User> GetUserById(int id)
        {
            try
            {
                User? user = _dbContext.Users.FirstOrDefault(user => user.Id == id);

                if (user is null)
                {
                    return Result<User>.Failure(Error.WrongUserId);
                }

                return Result<User>.Success(user);
            }
            catch (Exception exception)
            {
                var logErrorAction = "get user by id";
                return HandleUserException<User>(logErrorAction, exception);
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
            catch (Exception exception)
            {
                var logErrorAction = "get user by nickname";
                return HandleUserException<User>(logErrorAction, exception);
            }
        }

        private Result<bool> VerifyUserPassword(string userPassword, string password)
        {
            try
            {
                var result = _passwordHasherService.Verify(userPassword, password);

                return Result<bool>.Success(result);
            }
            catch (Exception exception)
            {
                var logErrorAction = "verify user password";
                return HandleUserException<bool>(logErrorAction, exception);
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
            catch (Exception exception)
            {
                var logErrorAction = "save password reset token";
                return HandleUserException<bool>(logErrorAction, exception);
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
            catch (Exception exception)
            {
                var logErrorAction = "change user password";
                return HandleUserException<bool>(logErrorAction, exception);
            }
        }

        public Result<bool> SaveUserRefreshToken(User user, string refreshToken)
        {
            try
            {
                user.RefreshToken = refreshToken;
                _dbContext.SaveChanges();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "save user refresh token";
                return HandleUserException<bool>(logErrorAction, exception);
            }
        }

        private Result<ResultType> HandleUserException<ResultType>(string logErrorAction, Exception exception)
        {
            var error = _errorHandlingService.HandleDatabaseError(
                exception,
                logErrorAction
            );

            return Result<ResultType>.Failure(error);
        }
    }
}