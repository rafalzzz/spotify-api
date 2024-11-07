namespace SpotifyApi.Responses
{
    public class UserPlaylistDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsPublic { get; set; }
        public bool IsOwner { get; set; }
        public bool IsCollaborator { get; set; }
        public List<int> SongIds { get; set; } = [];
    }
}