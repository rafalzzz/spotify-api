using System.ComponentModel;

namespace SpotifyApi.Enums
{
    public enum UserGender
    {
        [Description("male")]
        Male = 1,
        [Description("female")]
        Female = 2,
        [Description("non-binary-person")]
        NonBinaryPerson = 3,
        [Description("other")]
        Other = 4,
        [Description("not-provided")]
        NotProvided = 5
    }
}