using SpotifyApi.Enums;

namespace SpotifyApi.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Nickname { get; set; } = null!;
        public string DateOfBirth { get; set; } = null!;
        public UserGender Gender { get; set; }
        public bool Offers { get; set; }
        public bool ShareInformation { get; set; }
        public bool Terms { get; set; }
        public string RefreshToken { get; set; } = null!;
        public string PasswordResetToken { get; set; } = null!;
        public ICollection<Playlist> CreatedPlaylists { get; set; } = []; // Created playlists by user arr
        public ICollection<Playlist> CollaboratingPlaylists { get; set; } = []; // Playlists to which the user has been added as a collaborator
        public ICollection<Playlist> FavoritePlaylists { get; set; } = []; // // Playlists added to favorites
    }
}