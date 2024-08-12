using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IUserRegistrationService
    {
        Result<RegisterUser> ValidateRegistration(RegisterUser registerUserDto);
        Task<Result<RegisterUser>> CheckIfUserExists(RegisterUser registerUserDto);
        Result<RegisterUser> CreateUser(RegisterUser registerUserDto);
        ActionResult HandleRegistrationError(Error err);
    }
    public class UserRegistrationService(IRequestValidatorService requestValidatorService,
                                   IValidator<RegisterUser> registerUserValidator,
                                   IUserService userService) : IUserRegistrationService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<RegisterUser> _registerUserValidator = registerUserValidator;
        private readonly IUserService _userService = userService;

        public Result<RegisterUser> ValidateRegistration(RegisterUser registerUserDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(registerUserDto, _registerUserValidator);

            return validationResult.IsSuccess ? Result<RegisterUser>.Success(registerUserDto) :
                                               Result<RegisterUser>.Failure(new Error(ErrorType.Validation, validationResult.Error.Description));
        }

        public async Task<Result<RegisterUser>> CheckIfUserExists(RegisterUser registerUserDto)
        {
            bool exists = await _userService.UserExists(registerUserDto.Email, registerUserDto.Nickname);
            return !exists ? Result<RegisterUser>.Success(registerUserDto) :
                             Result<RegisterUser>.Failure(Error.UserAlreadyExist);
        }

        public Result<RegisterUser> CreateUser(RegisterUser registerUserDto)
        {
            var id = _userService.CreateUser(registerUserDto);
            return id != null ? Result<RegisterUser>.Success(registerUserDto) :
                                Result<RegisterUser>.Failure(Error.CreateUserFailed);
        }

        public ActionResult HandleRegistrationError(Error err)
        {
            return err.Type switch
            {
                ErrorType.Validation => new BadRequestObjectResult(err),
                ErrorType.UserAlreadyExist => new ConflictObjectResult(err.Description),
                ErrorType.Failure => new ObjectResult(err.Description) { StatusCode = StatusCodes.Status500InternalServerError },
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}
