using SpotifyApi.Entities;
using SpotifyApi.Enums;

namespace SpotifyApi.Classes
{
    public class LoginUserResult(LoginUserError? error, User? user)
    {
        public LoginUserError? Error { get; set; } = error;
        public User? User { get; set; } = user;
    }
}