using SpotifyApi.Services;

namespace SpotifyApi.DependencyInjection
{
    public static class ServicesCollection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRegistrationService, UserRegistrationService>();
            services.AddScoped<IUserLoginService, UserLoginService>();
            services.AddScoped<IAccessTokenService, AccessTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IPasswordResetService, PasswordResetService>();
            services.AddScoped<ITracksService, TracksService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordResetCompleteService, PasswordResetCompleteService>();
            services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
            services.AddScoped<IPlaylistService, PlaylistService>();
            services.AddScoped<IPlaylistCreationService, PlaylistCreationService>();
            services.AddScoped<IPlaylistEditionService, PlaylistEditionService>();

            services.AddTransient<IRequestValidatorService, RequestValidatorService>();
            services.AddTransient<IPasswordHasherService, PasswordHasherService>();
            services.AddTransient<ICookiesService, CookiesService>();
            services.AddTransient<IEmailService, EmailService>();

            return services;
        }
    }
}
