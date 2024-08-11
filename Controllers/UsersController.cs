using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using SpotifyApi.Variables;
using SpotifyApi.Requests;
using SpotifyApi.Services;
using SpotifyApi.Utilities;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.User)]
    public class UserController(
        IRequestValidatorService requestValidatorService,
        IValidator<RegisterUser> registerUserValidator,
        IUserRegistrationService userRegistrationService
    ) : ControllerBase
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<RegisterUser> _registerUserValidator = registerUserValidator;
        private readonly IUserRegistrationService _userRegistrationService = userRegistrationService;

        [HttpPost]
        public async Task<ActionResult> Register([FromBody] RegisterUser registerUserDto)
        {
            return await _userRegistrationService.ValidateRegistration(registerUserDto)
                .BindAsync(async _ => await _userRegistrationService.CheckIfUserExists(registerUserDto))
                .ThenBind(_ => _userRegistrationService.CreateUser(registerUserDto))
                .MatchAsync(
                    _ => Ok(),
                    _userRegistrationService.HandleRegistrationError
                );
        }
    }
}