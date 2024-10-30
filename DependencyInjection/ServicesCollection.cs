using SpotifyApi.Services;

namespace SpotifyApi.DependencyInjection
{
    public static class ServicesCollection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
            services.AddSingleton<ICookiesService, CookiesService>();
            services.AddSingleton<IJwtService, JwtService>();
            services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
            services.AddSingleton<IRequestValidatorService, RequestValidatorService>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRegistrationService, UserRegistrationService>();
            services.AddScoped<IUserLoginService, UserLoginService>();
            services.AddScoped<IAccessTokenService, AccessTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IPasswordResetService, PasswordResetService>();
            services.AddScoped<IPasswordResetCompleteService, PasswordResetCompleteService>();
            services.AddScoped<ITracksService, TracksService>();
            services.AddScoped<IPlaylistService, PlaylistService>();
            services.AddScoped<IPlaylistCreationService, PlaylistCreationService>();
            services.AddScoped<IPlaylistEditionService, PlaylistEditionService>();
            services.AddScoped<IPlaylistCollaboratorService, PlaylistCollaboratorService>();

            services.AddTransient<IEmailService, EmailService>();

            return services;
        }
    }
}
