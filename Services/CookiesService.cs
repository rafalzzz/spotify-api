namespace SpotifyApi.Services
{
    public interface ICookiesService
    {

        CookieOptions CreateCookieOptions(DateTimeOffset tokenLifeTime);
    }

    public class CookiesService : ICookiesService
    {

        public CookiesService()
        {
        }

        public CookieOptions CreateCookieOptions(DateTimeOffset tokenLifeTime)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = tokenLifeTime
            };

            return cookieOptions;
        }
    }
}