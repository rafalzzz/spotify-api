using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using SpotifyApi.Variables;
using SpotifyApi.Requests;
using SpotifyApi.Services;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.User)]
    public class UserController(
        IRequestValidatorService requestValidatorService,
        IValidator<RegisterUser> registerUserValidator,
        IUserService userService
    ) : ControllerBase
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<RegisterUser> _registerUserValidator = registerUserValidator;
        private readonly IUserService _userService = userService;

        [HttpPost]
        public async Task<ActionResult> Register(
                [FromBody] RegisterUser registerUserDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(registerUserDto, _registerUserValidator);
            if (validationResult is BadRequestObjectResult)
            {
                return validationResult;
            }

            bool userAlreadyExist = await _userService.UserExists(registerUserDto.Email, registerUserDto.Nickname);

            if (userAlreadyExist)
            {
                return Conflict("User with the provided email address or nickname already exists");
            }

            var id = _userService.CreateUser(registerUserDto);
            if (id is null)
            {
                return StatusCode(500, "Something went wrong, please try again");
            }

            return Ok();
        }
    }
}