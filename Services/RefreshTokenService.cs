using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
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
    }

    public class RefreshTokenService(
        IJwtService jwtService,
        IOptions<JwtSettings> refreshTokenSettings,
        IErrorHandlingService errorHandlingService,
        ICookiesService cookiesService
    ) : IRefreshTokenService
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly JwtSettings _refreshTokenSettings = refreshTokenSettings.Value;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;
        private readonly ICookiesService _cookiesService = cookiesService;

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
    }
}
