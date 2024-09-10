using SpotifyApi.Classes;

namespace SpotifyApi.DependencyInjection
{
    public static class ConfigurationsCollection
    {
        public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            var servicesSettings = configuration.GetSection("Services");
            services.Configure<ServiceSettings>(servicesSettings);

            var passwordResetTokenSettings = configuration.GetSection("PasswordResetTokenSettings");
            services.Configure<JwtSettings>(passwordResetTokenSettings);

            var accessTokenSettings = configuration.GetSection("AccessTokenSettings");
            services.Configure<JwtSettings>(accessTokenSettings);

            var refreshTokenSettings = configuration.GetSection("RefreshTokenSettings");
            services.Configure<JwtSettings>(refreshTokenSettings);

            return services;
        }
    }
}