using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using SpotifyApi.Classes;
using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;
using SpotifyApi.Variables;

namespace SpotifyApi.Services
{
    public interface IPasswordResetCompleteService
    {
        Result<PasswordResetComplete> ValidatePasswordResetCompleteRequest(PasswordResetComplete passwordResetCompleteDto);
        Result<User> ValidateToken(string token);
        ActionResult HandlePasswordResetCompleteError(Error error);
    }

    public class PasswordResetCompleteService(
        IRequestValidatorService requestValidatorService,
        IValidator<PasswordResetComplete> passwordResetCompleteValidator,
        IUserService userService,
        IJwtService jwtService,
        IOptions<JwtSettings> passwordResetTokenSettings
        ) : IPasswordResetCompleteService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<PasswordResetComplete> _passwordResetCompleteValidator = passwordResetCompleteValidator;
        private readonly IUserService _userService = userService;
        private readonly IJwtService _jwtService = jwtService;
        private readonly JwtSettings _passwordResetSettings = passwordResetTokenSettings.Value;

        public Result<PasswordResetComplete> ValidatePasswordResetCompleteRequest(PasswordResetComplete passwordResetCompleteDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(passwordResetCompleteDto, _passwordResetCompleteValidator);

            return validationResult.IsSuccess
                ? Result<PasswordResetComplete>.Success(passwordResetCompleteDto)
                : Result<PasswordResetComplete>.Failure(
                    new Error(ErrorType.Validation, validationResult.Error.Description)
                );
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
                ValidIssuer = _passwordResetSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _passwordResetSettings.Audience,
                ValidateLifetime = true
            };
        }

        private Result<string> GetEmailFromJwtToken(JwtSecurityToken jwtSecurityToken)
        {
            var emailClaim = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim == null)
            {
                return Result<string>.Failure(Error.InvalidToken);
            }

            return Result<string>.Success(emailClaim.Value);
        }

        public Result<User> ValidateToken(string token)
        {
            var passwordResetSecretKey = GetPasswordResetSecretKey();
            var key = GetSigningCredentialsKey(passwordResetSecretKey);
            var tokenValidationParameters = CreateTokenValidationParameters(key);

            return _jwtService.ValidateJwtToken(token, tokenValidationParameters)
                .Bind(GetEmailFromJwtToken)
                .Bind(_userService.GetUserByEmail);
        }

        public ActionResult HandlePasswordResetCompleteError(Error error)
        {
            return error.Type switch
            {
                ErrorType.Validation => new BadRequestObjectResult(error),
                ErrorType.InvalidToken => new BadRequestObjectResult(error.Description),
                ErrorType.TokenExpired => new BadRequestObjectResult(error.Description),
                ErrorType.WrongLogin => new BadRequestObjectResult(Error.InvalidToken.Description),
                _ => new ObjectResult("An unexpected error occurred: " + error.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}