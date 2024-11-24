namespace SpotifyApi.Classes
{
    public class Track
    {
        public required int Id { get; set; }
        public required string TrackName { get; set; } = string.Empty;
        public required string ArtistName { get; set; } = string.Empty;
    }
}