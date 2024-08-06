using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SpotifyApi.Variables;
using SpotifyApi.Classes;
using SpotifyApi.Enums;

namespace SpotifyApi.Services
{
    public interface IPasswordResetCompleteService
    {
        (string? validationError, string? email) ValidateToken(string token);
    }

    public class PasswordResetCompleteService : IPasswordResetCompleteService
    {
        private readonly IJwtService _jwtService;
        private readonly PasswordResetSettings _passwordResetSettings;
        private readonly IEmailService _emailService;

        public PasswordResetCompleteService(
            IJwtService jwtService,
            IOptions<PasswordResetSettings> passwordResetSettings,
            IEmailService emailService
            )
        {
            _jwtService = jwtService;
            _passwordResetSettings = passwordResetSettings.Value;
            _emailService = emailService;
        }

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
            tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

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
                string? passwordResetSecretKey = GetPasswordResetSecretKey();
                SecurityKey? key = GetSigningCredentialsKey(passwordResetSecretKey);
                TokenValidationParameters? tokenValidationParameters = CreateTokenValidationParameters(key);
                JwtSecurityToken jwtSecurityToken = ValidateJwtToken(token, tokenValidationParameters);
                Claim? emailClaim = GetEmailClaim(jwtSecurityToken);

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
            PasswordResetTokenValidationResult validationResult = GetEmailFromPasswordResetToken(token);

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