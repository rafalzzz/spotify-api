using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SpotifyApi.Classes;
using SpotifyApi.Entities;
using SpotifyApi.Variables;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IRefreshTokenService
    {
        Result<string> Generate(User user);
        CookieOptions GetRefreshTokenCookieOptions();
        Result<User> ValidateToken(string token);
        ActionResult HandleRefreshTokenError(Error error);


    }

    public class RefreshTokenService(
        IJwtService jwtService,
        IOptions<JwtSettings> refreshTokenSettings,
        IErrorHandlingService errorHandlingService,
        ICookiesService cookiesService,
        IUserService userService
    ) : IRefreshTokenService
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly JwtSettings _refreshTokenSettings = refreshTokenSettings.Value;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;
        private readonly ICookiesService _cookiesService = cookiesService;
        private readonly IUserService _userService = userService;

        private static List<Claim> GetClaims(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, user.Id.ToString())
            };

            return claims;
        }

        public Result<string> Generate(User user)
        {
            var claims = GetClaims(user);
            var refreshTokenSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.RefreshTokenSecretKey);
            var expires = DateTime.Now.AddDays(_refreshTokenSettings.TokenLifeTime);

            if (string.IsNullOrEmpty(refreshTokenSecretKey))
            {
                var configurationError = _errorHandlingService.HandleConfigurationError();
                return Result<string>.Failure(configurationError);
            }

            var token = _jwtService.GenerateToken(
                claims,
                _refreshTokenSettings.Issuer,
                _refreshTokenSettings.Audience,
                refreshTokenSecretKey,
                expires
            );

            return Result<string>.Success(token);
        }

        public CookieOptions GetRefreshTokenCookieOptions()
        {
            var expires = DateTimeOffset.UtcNow.AddDays(_refreshTokenSettings.TokenLifeTime);
            return _cookiesService.CreateCookieOptions(expires);
        }

        public Result<User> ValidateToken(string token)
        {
            var passwordResetSecretKey = GetPasswordResetSecretKey();
            var key = GetSigningCredentialsKey(passwordResetSecretKey);
            var tokenValidationParameters = CreateTokenValidationParameters(key);

            return jwtService.ValidateJwtToken(token, tokenValidationParameters)
                .Bind(GetIdFromJwtToken)
                .Bind(_userService.GetUserById);
        }

        private static string? GetPasswordResetSecretKey()
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.PasswordResetTokenSecretKey);
        }

        private SecurityKey? GetSigningCredentialsKey(string? secretKey)
        {
            if (string.IsNullOrEmpty(secretKey))
            {
                return null;
            }

            return _jwtService.GetSigningCredentials(secretKey).Key;
        }

        private TokenValidationParameters? CreateTokenValidationParameters(SecurityKey? key)
        {
            if (key == null)
            {
                return null;
            }

            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _refreshTokenSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _refreshTokenSettings.Audience,
                ValidateLifetime = true
            };
        }

        private Result<string> GetIdFromJwtToken(JwtSecurityToken jwtSecurityToken)
        {
            var userIdClaim = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

            if (userIdClaim == null)
            {
                return Result<string>.Failure(Error.InvalidToken);
            }

            return Result<string>.Success(userIdClaim.Value);
        }

        public ActionResult HandleRefreshTokenError(Error error)
        {
            return error.Type switch
            {
                ErrorType.Validation => new BadRequestObjectResult(error),
                ErrorType.InvalidToken => new BadRequestObjectResult(error.Description),
                ErrorType.TokenExpired => new BadRequestObjectResult(error.Description),
                ErrorType.WrongId => new BadRequestObjectResult(Error.WrongId.Description),
                _ => new ObjectResult("An unexpected error occurred: " + error.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}
