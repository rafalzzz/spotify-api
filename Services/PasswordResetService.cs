using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SpotifyApi.Variables;
using SpotifyApi.Classes;

namespace SpotifyApi.Services
{
    public interface IPasswordResetService
    {
        string? GeneratePasswordResetToken(string userEmail);
        Task SendPasswordResetToken(string email, string token);
    }

    public class PasswordResetService(
        IJwtService jwtService,
        IOptions<PasswordResetSettings> passwordResetSettings,
        IEmailService emailService
            ) : IPasswordResetService
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly PasswordResetSettings _passwordResetSettings = passwordResetSettings.Value;
        private readonly IEmailService _emailService = emailService;

        private static List<Claim> GetPasswordResetTokenClaims(string userEmail)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, userEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            return claims;
        }

        public string? GeneratePasswordResetToken(string userEmail)
        {
            List<Claim> claims = GetPasswordResetTokenClaims(userEmail);
            string? passwordResetSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.PasswordResetSecretKey);

            if (passwordResetSecretKey == null)
            {
                return null;
            }

            DateTime expires = DateTime.Now.AddMinutes(_passwordResetSettings.TokenLifeTime);

            return _jwtService.GenerateToken(claims, _passwordResetSettings.Issuer, _passwordResetSettings.Audience, passwordResetSecretKey, expires);
        }

        public async Task SendPasswordResetToken(string email, string token)
        {
            string emailTitle = "Password reset";

            string? clientUrl = Environment.GetEnvironmentVariable(EnvironmentVariables.ClientUrl);
            string passwordResetUrl = $"{clientUrl}/password-reset/complete/{token}";
            string emailContent = $@"
                <html>
                    <body style='width: 100%;'>
                        <h3 style='text-align: center;'>To reset your password</h3>
                        <div style='text-align: center; margin-top: 10px;'>
                            <a href='{passwordResetUrl}' style='background-color: #4CAF50; color: white; padding: 14px 20px; text-align: center; text-decoration: none; display: inline-block; border: none; cursor: pointer;'>Open this link</a>
                        </div>
                    </body>
                </html>";


            await _emailService.SendEmailAsync(email, emailTitle, emailContent);
        }
    }
}