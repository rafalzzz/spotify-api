namespace SpotifyApi.Entities
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // max 50 characters
        public string Description { get; set; } = null!;  // max 300 characters
        public bool IsPublic { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public List<int> SongIds { get; set; } = []; // Added songs arr
        public ICollection<User> Collaborators { get; set; } = []; // Playlist collaborators arr
        public ICollection<User> FavoritedByUsers { get; set; } = []; // Added to favorites arr
    }
}