using SpotifyApi.Classes;

namespace SpotifyApi.Responses
{
    public class PlaylistWithSongsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsPublic { get; set; }
        public bool IsOwner { get; set; }
        public bool IsCollaborator { get; set; }
        public bool IsFavorite { get; set; }
        public List<Track> Songs { get; set; } = [];
    }
}