using Microsoft.AspNetCore.Mvc;
using SpotifyApi.Requests;
using SpotifyApi.Classes;
using SpotifyApi.Entities;
using SpotifyApi.Enums;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IUserService
    {
        Task<bool> UserExists(string email, string nickname);
        int? CreateUser(RegisterUser userDto);
        Result<User> VerifyUser(LoginUser loginUserDto);
        Result<User> GetUserByLogin(string login);
    }

    public class UserService(
        SpotifyDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        IPasswordResetService passwordResetService
            ) : IUserService
    {
        private readonly SpotifyDbContext _dbContext = dbContext;
        private readonly IPasswordHasherService _passwordHasherService = passwordHasherService;
        private readonly IPasswordResetService _passwordResetService = passwordResetService;

        public async Task<bool> UserExists(string email, string nickname)
        {
            return await _dbContext.UserExists(email, nickname);
        }

        public int? CreateUser(RegisterUser registerUserDto)
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

            return newUser.Id;
        }

        private Result<User?> GetUserByEmail(string email)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(user => user.Email == email);
                return Result<User?>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User?>.Failure(new Error(ErrorType.Database, "Database connection error: " + ex.Message));
            }
        }

        private Result<User?> GetUserByNickname(string nickname)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(user => user.Nickname == nickname);
                return Result<User?>.Success(user);
            }
            catch (Exception ex)
            {
                return Result<User?>.Failure(new Error(ErrorType.Database, "Database connection error: " + ex.Message));
            }
        }

        private Result<bool> VerifyUserPassword(string userPassword, string password)
        {
            try
            {
                bool result = _passwordHasherService.Verify(userPassword, password);
                return Result<bool>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(new Error(ErrorType.PasswordHashing, "Password hashing error: " + ex.Message));
            }
        }

        public Result<User> GetUserByLogin(string login)
        {
            var userResult = login.Contains('@') ? GetUserByEmail(login) : GetUserByNickname(login);

            if (!userResult.IsSuccess || userResult.Value == null)
            {
                return Result<User>.Failure(Error.WrongLogin);
            }

            return Result<User>.Success(userResult.Value);
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
    }
}