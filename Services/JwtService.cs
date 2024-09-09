using Microsoft.IdentityModel.Tokens;
using SpotifyApi.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;

namespace SpotifyApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(List<Claim> claims, string issuer, string audience, string secretKey, DateTime expires);
        SigningCredentials GetSigningCredentials(string secretKey);
        Result<JwtSecurityToken> ValidateJwtToken(string token, TokenValidationParameters? tokenValidationParameters);
    }

    public class JwtService(
        IErrorHandlingService errorHandlingService
    ) : IJwtService
    {
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        public SigningCredentials GetSigningCredentials(string secretKey)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public string GenerateToken(List<Claim> claims, string issuer, string audience, string secretKey, DateTime expires)
        {
            var creds = GetSigningCredentials(secretKey);

            JwtSecurityToken token = new(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Result<JwtSecurityToken> ValidateJwtToken(string token, TokenValidationParameters? tokenValidationParameters)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new();
                tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return Result<JwtSecurityToken>.Failure(Error.InvalidToken);
                }

                return Result<JwtSecurityToken>.Success(jwtSecurityToken);
            }
            catch (SecurityTokenExpiredException)
            {
                Console.WriteLine($"Token has expired. Time: {DateTime.Now}.");
                return Result<JwtSecurityToken>.Failure(Error.TokenHasExpired);
            }
            catch (SecurityException exception)
            {
                var error = HandleTokenValidationError("reset password token validation error", exception);
                return Result<JwtSecurityToken>.Failure(error);
            }
            catch (Exception exception)
            {
                var error = HandleTokenValidationError("validation token", exception);
                return Result<JwtSecurityToken>.Failure(error);
            }
        }

        private Error HandleTokenValidationError(string logErrorAction, Exception exception)
        {
            var initialErrorMessage = "Unexpected validation token error";
            return _errorHandlingService.HandleError(exception, ErrorType.InvalidToken, logErrorAction, initialErrorMessage);
        }
    }
}
