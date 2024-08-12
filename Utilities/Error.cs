namespace SpotifyApi.Utilities
{

    public record Error(ErrorType Type, object Description)
    {
        public static Error UserAlreadyExist = new(ErrorType.UserAlreadyExist, "User with the provided email address or nickname already exists");
        public static Error WrongLogin = new(ErrorType.WrongLogin, "Incorrect login");
        public static Error WrongPassword = new(ErrorType.WrongLogin, "Incorrect password");
    }
}