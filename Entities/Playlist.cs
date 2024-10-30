namespace SpotifyApi.Entities
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsPublic { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public List<int> SongIds { get; set; } = [];
        public ICollection<User> Collaborators { get; set; } = [];
        public ICollection<User> FavoritedByUsers { get; set; } = [];
    }
}