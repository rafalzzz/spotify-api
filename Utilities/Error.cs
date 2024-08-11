namespace SpotifyApi.Utilities
{

    public record Error(ErrorType Type, object Description)
    {
        public static Error UserAlreadyExist = new(ErrorType.UserAlreadyExist, "User with the provided email address or nickname already exists");
        public static Error CreateUserFailed = new(ErrorType.Failure, "An error occurred - failed to create the user, please try again");
    }
}