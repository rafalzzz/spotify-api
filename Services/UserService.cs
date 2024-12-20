using Microsoft.EntityFrameworkCore;
using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IUserService
    {
        Task<Result<bool>> UserExists(string email, string nickname);
        Task<Result<User>> CreateUser(RegisterUser registerUserDto);
        Task<Result<User>> VerifyUser(LoginUser loginUserDto);
        Task<Result<User>> GetUserByEmail(string email);
        Task<Result<User>> GetUserById(int id);
        Task<Result<User>> GetUserByLogin(string login);
        Task<Result<bool>> SavePasswordResetToken(string token, User user);
        Task<Result<bool>> ChangeUserPassword(User user, string token, string password);
        Task<Result<bool>> SaveUserRefreshTokenAsync(User user, string refreshToken);
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
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public async Task<Result<User>> CreateUser(RegisterUser registerUserDto)
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

                await _dbContext.Users.AddAsync(newUser);
                await _dbContext.SaveChangesAsync();

                return Result<User>.Success(newUser);
            }
            catch (Exception exception)
            {
                var logErrorAction = "create user";
                return _errorHandlingService.HandleDatabaseException<User>(logErrorAction, exception);
            }
        }

        public async Task<Result<User>> GetUserByEmail(string email)
        {
            try
            {
                User? user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);

                if (user is null)
                {
                    return Result<User>.Failure(Error.WrongLogin);
                }

                return Result<User>.Success(user);
            }
            catch (Exception exception)
            {
                var logErrorAction = "get user by email";
                return _errorHandlingService.HandleDatabaseException<User>(logErrorAction, exception);
            }
        }

        public async Task<Result<User>> GetUserById(int id)
        {
            try
            {
                User? user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);

                if (user is null)
                {
                    return Result<User>.Failure(Error.WrongUserId);
                }

                return Result<User>.Success(user);
            }
            catch (Exception exception)
            {
                var logErrorAction = "get user by id";
                return _errorHandlingService.HandleDatabaseException<User>(logErrorAction, exception);
            }
        }

        private async Task<Result<User>> GetUserByNickname(string nickname)
        {
            try
            {
                User? user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Nickname == nickname);

                if (user is null)
                {
                    return Result<User>.Failure(Error.WrongLogin);
                }

                return Result<User>.Success(user);
            }
            catch (Exception exception)
            {
                var logErrorAction = "get user by nickname";
                return _errorHandlingService.HandleDatabaseException<User>(logErrorAction, exception);
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
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public async Task<Result<User>> GetUserByLogin(string login)
        {
            var userResult = login.Contains('@') ? await GetUserByEmail(login) : await GetUserByNickname(login);

            return userResult.IsSuccess ? Result<User>.Success(userResult.Value!) :
                Result<User>.Failure(userResult.Error);
        }

        public async Task<Result<User>> VerifyUser(LoginUser loginUserDto)
        {
            var userResult = await GetUserByLogin(loginUserDto.Login);

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

        public async Task<Result<bool>> SavePasswordResetToken(string token, User user)
        {
            try
            {
                user.PasswordResetToken = token;
                await _dbContext.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "save password reset token";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public async Task<Result<bool>> ChangeUserPassword(User user, string token, string password)
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

                await _dbContext.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "change user password";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }

        public async Task<Result<bool>> SaveUserRefreshTokenAsync(User user, string refreshToken)
        {
            try
            {
                user.RefreshToken = refreshToken;
                await _dbContext.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "save user refresh token";
                return _errorHandlingService.HandleDatabaseException<bool>(logErrorAction, exception);
            }
        }
    }
}