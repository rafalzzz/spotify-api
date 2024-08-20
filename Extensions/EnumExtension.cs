using System.ComponentModel;
using System.Reflection;

namespace SpotifyApi.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value) =>
            value.GetType()
                 .GetField(value.ToString())?
                 .GetCustomAttribute<DescriptionAttribute>()?
                 .Description ?? value.ToString();
    }
}