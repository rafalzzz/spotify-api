
namespace SpotifyApi.Requests
{
    public class LoginUser
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }

    }
}