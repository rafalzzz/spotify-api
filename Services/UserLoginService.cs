using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IUserLoginService
    {
        Result<LoginUser> ValidateLogin(LoginUser registerUserDto);
        Result<LoginUser> CheckLoginAndPassword(LoginUser loginUserDto);
        ActionResult HandleLoginError(Error err);
    }
    public class UserLoginService(
        IRequestValidatorService requestValidatorService,
        IValidator<LoginUser> loginUserValidator,
        IUserService userService
        ) : IUserLoginService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<LoginUser> _loginUserValidator = loginUserValidator;
        private readonly IUserService _userService = userService;

        public Result<LoginUser> ValidateLogin(LoginUser loginUserDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(loginUserDto, _loginUserValidator);

            return validationResult.IsSuccess ? Result<LoginUser>.Success(loginUserDto) :
                                               Result<LoginUser>.Failure(new Error(ErrorType.Validation, validationResult.Error.Description));
        }

        public Result<LoginUser> CheckLoginAndPassword(LoginUser loginUserDto)
        {
            var result = _userService.VerifyUser(loginUserDto);

            return result.IsSuccess ? Result<LoginUser>.Success(loginUserDto) :
            Result<LoginUser>.Failure(new Error(result.Error.Type, result.Error.Description));
        }

        public ActionResult HandleLoginError(Error err)
        {
            return err.Type switch
            {
                ErrorType.Validation => new BadRequestObjectResult(err),
                ErrorType.WrongLogin => new ConflictObjectResult(err.Description),
                ErrorType.WrongPassword => new BadRequestObjectResult(err),
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}
