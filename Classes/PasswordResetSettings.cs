namespace SpotifyApi.Classes
{
    public class PasswordResetSettings
    {
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int TokenLifeTime { get; set; }
    }
}
