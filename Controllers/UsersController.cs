using Microsoft.AspNetCore.Mvc;
using SpotifyApi.Variables;
using SpotifyApi.Requests;
using SpotifyApi.Services;
using SpotifyApi.Utilities;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.User)]
    public class UserController(
        IUserRegistrationService userRegistrationService,
        IUserLoginService userLoginService
    ) : ControllerBase
    {
        private readonly IUserRegistrationService _userRegistrationService = userRegistrationService;
        private readonly IUserLoginService _userLoginService = userLoginService;

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

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginUser loginUserDto)
        {
            return _userLoginService.ValidateLogin(loginUserDto)
            .Bind(_userLoginService.CheckLoginAndPassword)
            .Match(
                _ => Ok(),
                _userLoginService.HandleLoginError
            );
        }
    }
}