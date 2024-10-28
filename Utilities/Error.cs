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
        public static Error ConfigurationError = new(ErrorType.ConfigurationError, "Missing secretKey");
        public static Error WrongUserId = new(ErrorType.WrongUserId, "Incorrect user Id");
        public static Error WrongPlaylistId = new(ErrorType.WrongUserId, "Incorrect playlist Id");
        public static Error Unauthorized = new(ErrorType.Unauthorized, "Unauthorized");
        public static Error UserIsAlreadyAdded = new(ErrorType.UserIsAlreadyAdded, "User is already added to collaborators");
        public static Error UserIsNotAdded = new(ErrorType.UserIsNotAdded, "User is not added to collaborators");
    }
}