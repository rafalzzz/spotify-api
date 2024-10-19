namespace SpotifyApi.Requests
{
    public class CreatePlaylist
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsPublic { get; set; }
    }
}