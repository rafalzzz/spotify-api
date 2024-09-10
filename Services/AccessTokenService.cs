using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SpotifyApi.Classes;
using SpotifyApi.Entities;
using SpotifyApi.Variables;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IAccessTokenService
    {
        Result<string> Generate(User user);
        TokenValidationParameters GetJwtBearerSettings();
    }

    public class AccessTokenService(
        IJwtService jwtService,
        IOptions<JwtSettings> accessTokenSettings,
        IErrorHandlingService errorHandlingService
    ) : IAccessTokenService
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly JwtSettings _accessTokenSettings = accessTokenSettings.Value;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        private static List<Claim> GetClaims(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Nickname),
                new(JwtRegisteredClaimNames.Jti, user.Id.ToString())
            };

            return claims;
        }

        public Result<string> Generate(User user)
        {
            var claims = GetClaims(user);
            var accessTokenSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.AccessTokenSecretKey);
            var expires = DateTime.Now.AddMinutes(_accessTokenSettings.TokenLifeTime);

            if (string.IsNullOrEmpty(accessTokenSecretKey))
            {
                var configurationError = _errorHandlingService.HandleConfigurationError();
                return Result<string>.Failure(configurationError);
            }

            var token = _jwtService.GenerateToken(
                claims,
                _accessTokenSettings.Issuer,
                _accessTokenSettings.Audience,
                accessTokenSecretKey,
                expires
            );

            return Result<string>.Success(token);
        }

        public TokenValidationParameters GetJwtBearerSettings()
        {
            var accessTokenSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.AccessTokenSecretKey);

            if (string.IsNullOrEmpty(accessTokenSecretKey))
            {
                var configurationError = _errorHandlingService.HandleConfigurationError();
                throw new InvalidOperationException(configurationError.Description.ToString());
            }

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _accessTokenSettings.Issuer,
                ValidAudience = _accessTokenSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(accessTokenSecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
