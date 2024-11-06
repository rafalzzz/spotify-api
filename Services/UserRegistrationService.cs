using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IUserRegistrationService
    {
        Result<RegisterUser> ValidateRegistration(RegisterUser registerUserDto);
        Task<Result<RegisterUser>> CheckIfUserExists(RegisterUser registerUserDto);
        Task<Result<User>> CreateUser(RegisterUser registerUserDto);
        ActionResult HandleRegistrationError(Error err);
    }

    public class UserRegistrationService(
        IRequestValidatorService requestValidatorService,
        IValidator<RegisterUser> registerUserValidator,
        IUserService userService
    ) : IUserRegistrationService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<RegisterUser> _registerUserValidator = registerUserValidator;
        private readonly IUserService _userService = userService;

        public Result<RegisterUser> ValidateRegistration(RegisterUser registerUserDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(
                registerUserDto,
                _registerUserValidator
            );

            return validationResult.IsSuccess ? Result<RegisterUser>.Success(registerUserDto) :
                Result<RegisterUser>.Failure(validationResult.Error);
        }

        public async Task<Result<RegisterUser>> CheckIfUserExists(RegisterUser registerUserDto)
        {
            var userExistsResult = await _userService.UserExists(
                registerUserDto.Email,
                registerUserDto.Nickname
            );

            if (userExistsResult.IsSuccess && !userExistsResult.Value)
            {
                return Result<RegisterUser>.Success(registerUserDto);
            }

            if (userExistsResult.IsSuccess && userExistsResult.Value)
            {
                return Result<RegisterUser>.Failure(Error.UserAlreadyExist);
            }

            return Result<RegisterUser>.Failure(userExistsResult.Error);
        }

        public async Task<Result<User>> CreateUser(RegisterUser registerUserDto)
        {
            var createUserResult = await _userService.CreateUser(registerUserDto);

            return createUserResult.IsSuccess ?
                Result<User>.Success(createUserResult.Value) :
                Result<User>.Failure(createUserResult.Error);
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
