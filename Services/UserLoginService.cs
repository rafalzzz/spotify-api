using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using SpotifyApi.Classes;
using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IUserLoginService
    {
        Result<LoginUser> ValidateLogin(LoginUser registerUserDto);
        Task<Result<User>> CheckLoginAndPassword(LoginUser loginUserDto);
        Task<Result<TokenResult>> GenerateTokens(User user);
        ActionResult HandleLoginError(Error err);
    }

    public class UserLoginService(
        IRequestValidatorService requestValidatorService,
        IValidator<LoginUser> loginUserValidator,
        IUserService userService,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService
    ) : IUserLoginService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<LoginUser> _loginUserValidator = loginUserValidator;
        private readonly IUserService _userService = userService;
        private readonly IAccessTokenService _accessTokenService = accessTokenService;
        private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;

        public Result<LoginUser> ValidateLogin(LoginUser loginUserDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(loginUserDto, _loginUserValidator);

            return validationResult.IsSuccess ? Result<LoginUser>.Success(loginUserDto) :
                Result<LoginUser>.Failure(
                    new Error(ErrorType.Validation, validationResult.Error.Description)
                );
        }

        public async Task<Result<User>> CheckLoginAndPassword(LoginUser loginUserDto)
        {
            var verifyUserResult = await _userService.VerifyUser(loginUserDto);

            return verifyUserResult.IsSuccess ? Result<User>.Success(verifyUserResult.Value) :
                Result<User>.Failure(verifyUserResult.Error);
        }

        public async Task<Result<TokenResult>> GenerateTokens(User user)
        {
            var jwtTokenResult = _accessTokenService.Generate(user);
            var refreshTokenResult = _refreshTokenService.Generate(user);

            if (!jwtTokenResult.IsSuccess || !refreshTokenResult.IsSuccess)
            {
                return Result<TokenResult>.Failure(Error.ConfigurationError);
            }

            var saveRefreshTokenResult = await _userService.SaveUserRefreshTokenAsync(user, refreshTokenResult.Value);

            if (!saveRefreshTokenResult.IsSuccess)
            {
                return Result<TokenResult>.Failure(saveRefreshTokenResult.Error);
            }

            var tokenResult = new TokenResult(jwtTokenResult.Value, refreshTokenResult.Value);
            return Result<TokenResult>.Success(tokenResult);
        }

        public ActionResult HandleLoginError(Error err)
        {
            return err.Type switch
            {
                ErrorType.Validation => new BadRequestObjectResult(err),
                ErrorType.WrongLogin => new ConflictObjectResult(err.Description),
                ErrorType.WrongPassword => new BadRequestObjectResult(err.Description),
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}
