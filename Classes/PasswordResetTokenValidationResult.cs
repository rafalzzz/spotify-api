using SpotifyApi.Enums;

namespace SpotifyApi.Classes
{
    public class PasswordResetTokenValidationResult
    {
        public string? Email { get; set; }
        public VerifyPasswordResetTokenError? ErrorStatus { get; set; }
    }
}