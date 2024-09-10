using Microsoft.AspNetCore.Authentication.JwtBearer;
using SpotifyApi.Services;

namespace SpotifyApi.DependencyInjection
{
    public static class AuthenticationConfiguration
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var accessTokenService = serviceProvider.GetRequiredService<IAccessTokenService>();

                options.TokenValidationParameters = accessTokenService.GetJwtBearerSettings();
            });

            return services;
        }
    }
}