using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using SpotifyApi.Classes;
using SpotifyApi.Entities;
using SpotifyApi.Variables;

namespace SpotifyApi.Services
{
    public interface IAccessTokenService
    {

    }

    public class AccessTokenService(
        IJwtService jwtService,
        IOptions<JwtSettings> accessTokenSettings
    ) : IAccessTokenService
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly JwtSettings _accessTokenSettings = accessTokenSettings.Value;

        private static List<Claim> GetClaims(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Nickname),
                new(JwtRegisteredClaimNames.Jti, user.Id.ToString())
            };

            return claims;
        }

        public string? GenerateJwtToken(User user)
        {
            var claims = GetClaims(user);
            var accessTokenSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.AccessTokenSecretKey);
            var expires = DateTime.Now.AddMinutes(_accessTokenSettings.TokenLifeTime);

            if (accessTokenSecretKey == null)
            {
                return null;
            }

            var token = _jwtService.GenerateToken(
                claims,
                _accessTokenSettings.Issuer,
                _accessTokenSettings.Audience,
                accessTokenSecretKey,
                expires
            );

            return token;
        }
    }
}
