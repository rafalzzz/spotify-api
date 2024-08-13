namespace SpotifyApi.Utilities
{
    public enum ErrorType
    {
        Validation = 0,
        Database = 1,
        Internal = 3,
        Failure = 4,
        UserAlreadyExist = 5,
        PasswordHashing = 6,
        WrongLogin = 7,
        WrongPassword = 8,
        WrongEmail = 9,
        GeneratePasswordResetTokenError = 10
    }
}