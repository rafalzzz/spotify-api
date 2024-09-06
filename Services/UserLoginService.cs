using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IUserLoginService
    {
        Result<LoginUser> ValidateLogin(LoginUser registerUserDto);
        Result<string> CheckLoginAndPassword(LoginUser loginUserDto);
        ActionResult HandleLoginError(Error err);
    }

    public class UserLoginService(
        IRequestValidatorService requestValidatorService,
        IValidator<LoginUser> loginUserValidator,
        IUserService userService,
        IAccessTokenService accessTokenService
    ) : IUserLoginService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<LoginUser> _loginUserValidator = loginUserValidator;
        private readonly IUserService _userService = userService;
        private readonly IAccessTokenService _accessTokenService = accessTokenService;

        public Result<LoginUser> ValidateLogin(LoginUser loginUserDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(loginUserDto, _loginUserValidator);

            return validationResult.IsSuccess ? Result<LoginUser>.Success(loginUserDto) :
                Result<LoginUser>.Failure(
                    new Error(ErrorType.Validation, validationResult.Error.Description)
                );
        }

        public Result<string> CheckLoginAndPassword(LoginUser loginUserDto)
        {
            var verifyUserResult = _userService.VerifyUser(loginUserDto);

            if (!verifyUserResult.IsSuccess)
            {
                return Result<string>.Failure(verifyUserResult.Error);
            }

            var jwtTokenResult = _accessTokenService.GenerateJwtToken(verifyUserResult.Value);

            return jwtTokenResult.IsSuccess ? Result<string>.Success(jwtTokenResult.Value) :
                Result<string>.Failure(jwtTokenResult.Error);
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
