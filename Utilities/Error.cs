namespace SpotifyApi.Utilities
{

    public record Error(ErrorType Type, object Description)
    {
        public static Error UserAlreadyExist = new(ErrorType.UserAlreadyExist, "User with the provided email address or nickname already exists");
        public static Error WrongLogin = new(ErrorType.WrongLogin, "Incorrect login");
        public static Error WrongPassword = new(ErrorType.WrongPassword, "Incorrect password");
        public static Error WrongEmail = new(ErrorType.WrongEmail, "Account with the provided email does not exist");
        public static Error GeneratePasswordResetTokenError = new(ErrorType.GeneratePasswordResetTokenError, "Failed to generate password reset token");
        public static Error TokenHasExpired = new(ErrorType.TokenExpired, "Token has expired");
        public static Error InvalidToken = new(ErrorType.InvalidToken, "Invalid token");
        public static Error ApiItunesError = new(ErrorType.ApiItunes, "Error calling iTunes API");
    }
}