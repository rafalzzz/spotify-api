using SpotifyApi.Services;

namespace SpotifyApi.DependencyInjection
{
    public static class ServicesCollection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUserRegistrationService, UserRegistrationService>();
            services.AddTransient<IUserLoginService, UserLoginService>();
            services.AddTransient<IAccessTokenService, AccessTokenService>();
            services.AddTransient<IRefreshTokenService, RefreshTokenService>();
            services.AddTransient<IPasswordHasherService, PasswordHasherService>();
            services.AddTransient<IPasswordResetService, PasswordResetService>();
            services.AddTransient<ITracksService, TracksService>();
            services.AddTransient<IJwtService, JwtService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IPasswordResetCompleteService, PasswordResetCompleteService>();
            services.AddTransient<IRequestValidatorService, RequestValidatorService>();
            services.AddTransient<IErrorHandlingService, ErrorHandlingService>();
            services.AddTransient<ICookiesService, CookiesService>();
            services.AddScoped<IPlaylistService, PlaylistService>();
            services.AddScoped<IPlaylistCreationService, PlaylistCreationService>();
            services.AddScoped<IPlaylistEditionService, PlaylistEditionService>();

            return services;
        }
    }
}
