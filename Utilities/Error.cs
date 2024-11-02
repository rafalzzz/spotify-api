namespace SpotifyApi.Utilities
{

    public record Error(ErrorType Type, object Description)
    {
        public static readonly Error UserAlreadyExist = new(ErrorType.UserAlreadyExist, "User with the provided email address or nickname already exists");
        public static readonly Error WrongLogin = new(ErrorType.WrongLogin, "Incorrect login");
        public static readonly Error WrongPassword = new(ErrorType.WrongPassword, "Incorrect password");
        public static readonly Error WrongEmail = new(ErrorType.WrongEmail, "Account with the provided email does not exist");
        public static readonly Error GeneratePasswordResetTokenError = new(ErrorType.GeneratePasswordResetTokenError, "Failed to generate password reset token");
        public static readonly Error TokenHasExpired = new(ErrorType.TokenExpired, "Token has expired");
        public static readonly Error InvalidToken = new(ErrorType.InvalidToken, "Invalid token");
        public static readonly Error ApiItunesError = new(ErrorType.ApiItunes, "Error calling iTunes API");
        public static readonly Error ConfigurationError = new(ErrorType.ConfigurationError, "Missing secretKey");
        public static readonly Error WrongUserId = new(ErrorType.WrongUserId, "Incorrect user Id");
        public static readonly Error WrongPlaylistId = new(ErrorType.WrongUserId, "Incorrect playlist Id");
        public static readonly Error Unauthorized = new(ErrorType.Unauthorized, "Unauthorized");
        public static readonly Error UserIsAlreadyAdded = new(ErrorType.UserIsAlreadyAdded, "User is already added to collaborators");
        public static readonly Error UserIsNotAdded = new(ErrorType.UserIsNotAdded, "User is not added to collaborators");
        public static readonly Error SongIsAlreadyAdded = new(ErrorType.SongIsAlreadyAdded, "Song is already added to playlist");
        public static readonly Error SongIsNotAdded = new(ErrorType.SongIsNotAdded, "Song is not added to playlist");
    }
}