using System.Security;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SpotifyApi.Classes;
using SpotifyApi.Enums;
using SpotifyApi.Variables;

namespace SpotifyApi.Services
{
    public interface IPasswordResetCompleteService
    {
        (string? validationError, string? email) ValidateToken(string token);
    }

    public class PasswordResetCompleteService(
        IJwtService jwtService,
        IOptions<PasswordResetSettings> passwordResetSettings
    ) : IPasswordResetCompleteService
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly PasswordResetSettings _passwordResetSettings = passwordResetSettings.Value;

        private static string? GetPasswordResetSecretKey()
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.PasswordResetSecretKey);
        }

        private SecurityKey? GetSigningCredentialsKey(string? secretKey)
        {
            if (secretKey == null)
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
                ValidIssuer = _passwordResetSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _passwordResetSettings.Audience,
                ValidateLifetime = true
            };
        }

        private static JwtSecurityToken ValidateJwtToken(string token, TokenValidationParameters? tokenValidationParameters)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityException("Incorrect token");
            }

            return jwtSecurityToken;
        }

        private static Claim? GetEmailClaim(JwtSecurityToken? jwtSecurityToken)
        {
            if (jwtSecurityToken == null)
            {
                return null;
            }

            return jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        }

        private PasswordResetTokenValidationResult CreateErrorResult(VerifyPasswordResetTokenError status)
        {
            return new PasswordResetTokenValidationResult
            {
                Email = null,
                ErrorStatus = status
            };
        }

        private PasswordResetTokenValidationResult GetEmailFromPasswordResetToken(string token)
        {
            try
            {
                var passwordResetSecretKey = GetPasswordResetSecretKey();
                var key = GetSigningCredentialsKey(passwordResetSecretKey);
                var tokenValidationParameters = CreateTokenValidationParameters(key);
                var jwtSecurityToken = ValidateJwtToken(token, tokenValidationParameters);
                var emailClaim = GetEmailClaim(jwtSecurityToken);

                return new PasswordResetTokenValidationResult
                {
                    Email = emailClaim?.Value,
                    ErrorStatus = null
                };
            }
            catch (SecurityTokenExpiredException)
            {
                Console.WriteLine($"Token has expired. Time: {DateTime.Now}.");
                return CreateErrorResult(VerifyPasswordResetTokenError.TokenHasExpired);
            }
            catch (SecurityException exception)
            {
                Console.WriteLine($"Reset password token validation error. Time: {DateTime.Now}. Error message: {exception.Message}");
                return CreateErrorResult(VerifyPasswordResetTokenError.TokenValidationError);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Unexpected error during token validation. Time: {DateTime.Now}. Error message: {exception.Message}");
                return CreateErrorResult(VerifyPasswordResetTokenError.TokenValidationError);
            }
        }

        public (string? validationError, string? email) ValidateToken(string token)
        {
            var validationResult = GetEmailFromPasswordResetToken(token);

            if (validationResult.ErrorStatus.HasValue)
            {
                return validationResult.ErrorStatus.Value switch
                {
                    VerifyPasswordResetTokenError.TokenHasExpired => ("Token has expired", null),
                    VerifyPasswordResetTokenError.TokenValidationError => ("Invalid token", null),
                    _ => ("Unknown token error", null),
                };
            }

            return (null, validationResult.Email);
        }
    }
}