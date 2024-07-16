using SpotifyApi.Enums;

namespace SpotifyApi.Classes
{
    public class SearchTracksParams
    {
        public string Term { get; set; } = string.Empty;
        public Entity Entity { get; set; } = Entity.musicTrack;
        public int Limit { get; set; } = 10;
        public int Offset { get; set; } = 0;
    }
}