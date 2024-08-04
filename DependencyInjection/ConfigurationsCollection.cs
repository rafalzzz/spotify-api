using SpotifyApi.Services;
using SpotifyApi.Classes;

namespace SpotifyApi.DependencyInjection
{
    public static class ConfigurationsCollection
    {
        public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            var passwordResetSettings = configuration.GetSection("PasswordResetSettings");
            services.Configure<PasswordResetSettings>(passwordResetSettings);

            return services;
        }
    }
}