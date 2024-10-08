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
        IUserLoginService userLoginService,
        IRefreshTokenService refreshTokenService,
        IPasswordResetService passwordResetService,
        IPasswordResetCompleteService passwordResetCompleteService,
        IUserService userService
    ) : ControllerBase
    {
        private readonly IUserRegistrationService _userRegistrationService = userRegistrationService;
        private readonly IUserLoginService _userLoginService = userLoginService;
        private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
        private readonly IPasswordResetService _passwordResetService = passwordResetService;
        private readonly IPasswordResetCompleteService _passwordResetCompleteService = passwordResetCompleteService;
        private readonly IUserService _userService = userService;

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
            .Bind(_userLoginService.GenerateTokens)
            .Match(
                tokensResult =>
                {
                    var accessTokenCookieOptions = _refreshTokenService.GetRefreshTokenCookieOptions();
                    Response.Cookies.Append(CookieNames.RefreshToken, tokensResult.RefreshToken, accessTokenCookieOptions);

                    return Ok(tokensResult.AccessToken);
                },
                _userLoginService.HandleLoginError
            );
        }

        [HttpPost("password-reset")]
        public async Task<ActionResult> PasswordReset([FromBody] PasswordReset passwordResetDto)
        {
            return await _passwordResetService.ValidatePasswordResetRequest(passwordResetDto)
                .Bind(_passwordResetService.CheckIfUserExists)
                .BindAsync(_passwordResetService.GenerateAndSendPasswordResetToken)
                .MatchAsync(
                    _ => Ok(),
                    _passwordResetService.HandlePasswordResetError
                );
        }

        [HttpPut("password-reset-complete/{token}")]
        public ActionResult PasswordResetComplete([FromBody] PasswordResetComplete passwordResetCompleteDto, [FromRoute] string token)
        {
            return _passwordResetCompleteService.ValidatePasswordResetCompleteRequest(passwordResetCompleteDto)
                .Bind(_ => _passwordResetCompleteService.ValidateToken(token))
                .Bind(user => _userService.ChangeUserPassword(user, token, passwordResetCompleteDto.Password))
                .Match(
                    _ => Ok(),
                    _passwordResetCompleteService.HandlePasswordResetCompleteError
                );
        }

        [HttpGet("refresh-token/{token}")]
        public ActionResult RefreshAccessToken([FromRoute] string token)
        {
            return _refreshTokenService.ValidateToken(token)
                .Bind(_refreshTokenService.Generate)
                .Match(
                    Ok,
                    _refreshTokenService.HandleRefreshTokenError
                );
        }
    }
}