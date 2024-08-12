namespace SpotifyApi.Utilities
{
    public enum ErrorType
    {
        Validation = 0,
        Database = 1,
        UserAlreadyExist = 2,
        Failure = 3,
        PasswordHashing = 4,
        WrongLogin = 5,
        WrongPassword = 6,
    }
}