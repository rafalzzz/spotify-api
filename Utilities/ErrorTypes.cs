namespace SpotifyApi.Utilities
{
    public enum ErrorType
    {
        Validation = 0,
        Database = 1,
        Internal = 2,
        Failure = 3,
        UserAlreadyExist = 4,
        PasswordHashing = 5,
        WrongLogin = 6,
        WrongPassword = 7,
        WrongEmail = 8,
        GeneratePasswordResetTokenError = 9,
        TokenExpired = 10,
        InvalidToken = 11,
    }
}