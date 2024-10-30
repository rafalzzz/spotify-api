using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using FluentValidation;
using SpotifyApi.Classes;
using SpotifyApi.Entities;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;
using SpotifyApi.Variables;

namespace SpotifyApi.Services
{
    public interface IPasswordResetService
    {
        Result<PasswordReset> ValidatePasswordResetRequest(PasswordReset passwordResetDto);
        Result<User> CheckIfUserExists(PasswordReset passwordResetDto);
        Task<Result<bool>> GenerateAndSendPasswordResetToken(User user);
        public ActionResult HandlePasswordResetError(Error err);

    }

    public class PasswordResetService(
        IRequestValidatorService requestValidatorService,
        IValidator<PasswordReset> passwordResetValidator,
        IUserService userService,
        IJwtService jwtService,
        IOptions<JwtSettings> passwordResetTokenSettings,
        IEmailService emailService,
        IErrorHandlingService errorHandlingService
    ) : IPasswordResetService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<PasswordReset> _passwordResetValidator = passwordResetValidator;
        private readonly IUserService _userService = userService;
        private readonly IJwtService _jwtService = jwtService;
        private readonly JwtSettings _passwordResetSettings = passwordResetTokenSettings.Value;
        private readonly IEmailService _emailService = emailService;
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        public Result<PasswordReset> ValidatePasswordResetRequest(PasswordReset passwordResetDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(passwordResetDto, _passwordResetValidator);

            return validationResult.IsSuccess ? Result<PasswordReset>.Success(passwordResetDto)
                : Result<PasswordReset>.Failure(
                    new Error(ErrorType.Validation, validationResult.Error.Description)
                );
        }

        public Result<User> CheckIfUserExists(PasswordReset passwordResetDto)
        {
            var userResult = _userService.GetUserByLogin(passwordResetDto.Login);

            return userResult.IsSuccess ?
                Result<User>.Success(userResult.Value) :
                Result<User>.Failure(Error.WrongEmail);
        }

        private static List<Claim> GetClaims(string userEmail)
        {
            List<Claim> claims =
            [
                new Claim(ClaimTypes.Email, userEmail),
            ];

            return claims;
        }

        private string? Generate(string userEmail)
        {
            var claims = GetClaims(userEmail);
            var passwordResetSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.PasswordResetTokenSecretKey);

            if (string.IsNullOrEmpty(passwordResetSecretKey))
            {
                _errorHandlingService.HandleConfigurationError();
                return null;
            }

            var expires = DateTime.Now.AddMinutes(_passwordResetSettings.TokenLifeTime);

            return _jwtService.GenerateToken(
                claims,
                _passwordResetSettings.Issuer,
                _passwordResetSettings.Audience,
                passwordResetSecretKey,
                expires
            );
        }

        private async Task<Result<bool>> SendPasswordResetToken(string email, string token)
        {
            var emailTitle = "Password reset";
            var clientUrl = Environment.GetEnvironmentVariable(EnvironmentVariables.ClientUrl);
            var passwordResetUrl = $"{clientUrl}/password-reset/complete/{token}";
            var emailContent = $@"
                <html>
                    <body style='width: 100%;'>
                        <h3 style='text-align: center;'>To reset your password</h3>
                        <div style='text-align: center; margin-top: 10px;'>
                            <a href='{passwordResetUrl}' style='background-color: #4CAF50; color: white; padding: 14px 20px; text-align: center; text-decoration: none; display: inline-block; border: none; cursor: pointer;'>Open this link</a>
                        </div>
                    </body>
                </html>";

            try
            {
                await _emailService.SendEmailAsync(email, emailTitle, emailContent);

                return Result<bool>.Success(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "send password reset token";
                var initialErrorMessage = "Unexpected error occured during sending password reset token";

                var error = _errorHandlingService.HandleError(
                exception,
                ErrorType.Failure,
                logErrorAction,
                initialErrorMessage
                );

                return Result<bool>.Failure(error);
            }
        }

        public async Task<Result<bool>> GenerateAndSendPasswordResetToken(User user)
        {

            var token = Generate(user.Email);


            if (token == null)
            {
                return Result<bool>.Failure(Error.GeneratePasswordResetTokenError);
            }

            var savePasswordResetTokenResult = _userService.SavePasswordResetToken(token, user);

            if (!savePasswordResetTokenResult.IsSuccess)
            {
                Result<bool>.Failure(savePasswordResetTokenResult.Error);
            }

            var sendEmailResult = await SendPasswordResetToken(user.Email, token);

            return sendEmailResult.IsSuccess
                ? Result<bool>.Success(true)
                : Result<bool>.Failure(sendEmailResult.Error);
        }

        public ActionResult HandlePasswordResetError(Error err)
        {
            return err.Type switch
            {
                ErrorType.WrongLogin => new NotFoundObjectResult(err.Description),
                ErrorType.GeneratePasswordResetTokenError => new ObjectResult(err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                },
                _ => new ObjectResult("An unexpected error occurred: " + err.Description)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}