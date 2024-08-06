using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using SpotifyApi.Variables;
using SpotifyApi.Requests;
using SpotifyApi.Services;
using SpotifyApi.Enums;
using SpotifyApi.Classes;
using SpotifyApi.Entities;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.User)]
    public class UserController(
        IRequestValidatorService requestValidatorService,
        IValidator<RegisterUser> registerUserValidator,
        IValidator<LoginUser> loginUserValidator,
        IValidator<PasswordReset> passwordResetValidator,
        IValidator<PasswordResetComplete> passwordResetCompleteValidator,
        IUserService userService,
        IPasswordResetCompleteService passwordResetCompleteService
    ) : ControllerBase
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<RegisterUser> _registerUserValidator = registerUserValidator;
        private readonly IValidator<LoginUser> _loginUserValidator = loginUserValidator;
        private readonly IValidator<PasswordReset> _passwordResetValidator = passwordResetValidator;
        private readonly IValidator<PasswordResetComplete> _passwordResetCompleteValidator = passwordResetCompleteValidator;
        private readonly IUserService _userService = userService;
        private readonly IPasswordResetCompleteService _passwordResetCompleteService = passwordResetCompleteService;

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

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginUser loginUserDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(loginUserDto, _loginUserValidator);
            if (validationResult is BadRequestObjectResult)
            {
                return validationResult;
            }

            LoginUserResult result = _userService.VerifyUser(loginUserDto);

            if (result.Error == LoginUserError.WrongLogin)
            {
                return NotFound("Incorrect login");
            }

            if (result.Error == LoginUserError.WrongPassword)
            {
                return BadRequest("Incorrect password");
            }

            return Ok();
        }

        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset([FromBody] PasswordReset passwordResetDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(passwordResetDto, _passwordResetValidator);
            if (validationResult is BadRequestObjectResult)
            {
                return validationResult;
            }

            User? user = _userService.GetUserByLogin(passwordResetDto.Login);

            if (user is null)
            {
                return BadRequest("Account with the provided login doest not exist");
            }

            await _userService.GenerateAndSendPasswordResetToken(user);

            return Ok();
        }

        [HttpPut("password-reset-complete/{token}")]
        public ActionResult PasswordResetComplete([FromBody] PasswordResetComplete passwordResetCompleteDto, [FromRoute] string token)
        {
            var validationResult = _requestValidatorService.ValidateRequest(passwordResetCompleteDto, _passwordResetCompleteValidator);
            if (validationResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            (string? validationError, string? email) = _passwordResetCompleteService.ValidateToken(token);

            if (validationError != null || email == null)
            {
                return Unauthorized(validationError);
            }

            User? user = _userService.CheckUserPasswordResetToken(email, token);
            if (user == null)
            {
                return BadRequest("Invalid token");
            }

            _userService.ChangeUserPassword(user, passwordResetCompleteDto.Password);
            return Ok("Password has changed successfully");
        }
    }
}